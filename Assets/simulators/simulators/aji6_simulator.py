# flake8: noqa: E402, F401

import asyncio
import gc
import os
import platform
import sys
from datetime import datetime

sys.path.append(".")  # noqa
import threading
import time
from queue import Empty, Queue

import cv2
import httpx
import yaml
import zmq
import zmq.asyncio
from icecream import ic

from simulators.robots.aji6 import Aji6
from simulators.simulator_base import Simulator
from simulators.tasks import *

if platform.system() == "Windows":
    asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())


class Aji6Simulator(Simulator):
    """
    Based on DeepMind Control.
    """

    ZMQ_PROTOCOL = "tcp"
    IP_ADDRESS = "127.0.0.1"

    def __init__(
        self,
        config: dict,
        xml_path: str,
        keyframe: str,
        robot_pub_port: int,
        env_sub_port: int,
        capture: bool = False,
        save_results: bool = False,
    ):
        super().__init__(config["name"])
        self.client = httpx.AsyncClient()
        self.robot = Aji6(
            config=config,
            xml_path=xml_path,
            keyframe=keyframe,
        )
        self.terminate_flag = False

        self.context = zmq.asyncio.Context()
        self.robot_pub = self.context.socket(zmq.PUB)
        self.robot_pub.bind(f"{self.ZMQ_PROTOCOL}://{self.IP_ADDRESS}:{robot_pub_port}")

        self.env_sub = self.context.socket(zmq.SUB)
        self.env_sub.connect(f"{self.ZMQ_PROTOCOL}://{self.IP_ADDRESS}:{env_sub_port}")
        self.env_sub.setsockopt_string(zmq.SUBSCRIBE, "")

        self.env_queue = Queue()
        self.env_thread = threading.Thread(
            target=asyncio.run, args=(self.env_subscriber(),)
        )
        self.env_thread.start()

        self.capture_views = capture
        self.save_results = save_results
        self.save_dir = f"experiments/{self.robot.name}/simulator/{datetime.now().strftime('%Y-%m-%d_%H-%M-%S')}/"
        if self.save_results:
            os.makedirs(self.save_dir, exist_ok=True)
            with open(os.path.join(self.save_dir, "qpos.csv"), "w") as f:
                f.write("timestamp, qpos, joint_names\n")

        self.simulate_hz = config["memory_management"]["live_memory"]["hz"]

    async def run(self):
        if self.capture_views:
            self.tracking_view_window_name = "tracking_view"
            self.side_view_window_name = "side_view"
            self.egocentric_view_window_name = "egocentric_view"
            # self.depth_view_window_name = "depth_view"
            cv2.namedWindow(self.tracking_view_window_name)
            cv2.namedWindow(self.side_view_window_name)
            cv2.namedWindow(self.egocentric_view_window_name)
            # cv2.namedWindow(self.depth_view_window_name)
            cv2.setWindowProperty(
                self.tracking_view_window_name,
                cv2.WND_PROP_FULLSCREEN,
                cv2.WINDOW_FULLSCREEN,
            )
            cv2.setWindowProperty(
                self.side_view_window_name,
                cv2.WND_PROP_FULLSCREEN,
                cv2.WINDOW_FULLSCREEN,
            )
            cv2.setWindowProperty(
                self.egocentric_view_window_name,
                cv2.WND_PROP_FULLSCREEN,
                cv2.WINDOW_FULLSCREEN,
            )
            cv2.moveWindow(self.tracking_view_window_name, 0, 0)
            cv2.moveWindow(
                self.side_view_window_name, 640, 0
            )  # Adjust x-position as needed
            cv2.moveWindow(self.egocentric_view_window_name, 1280, 0)
            # cv2.setWindowProperty(
            #     self.depth_view_window_name,
            #     cv2.WND_PROP_FULLSCREEN,
            #     cv2.WINDOW_FULLSCREEN,
            # )

        while True:
            robot_geoms = self.robot.geoms
            data_obj = {
                self.robot.name: {
                    "timestamp": time.time(),
                    "body_geoms": self.robot.body_geoms,
                    "geoms": robot_geoms,
                    "geom_mapping": self.robot.geom_mapping,
                    "geom_xmat": self.robot.d.geom_xmat,
                    "vision": self.robot.vision,
                    "joint_mapping": self.robot.joint_mapping,
                    "joint_states": self.robot.joint_states,
                    "qpos": self.robot.d.qpos,
                    "qvel": self.robot.d.qvel,
                    "imu": self.robot.imu,
                    "contact": self.robot.contact,
                    "efc_force": self.robot.d.efc_force,
                    "force": self.robot.force,
                    "sites": self.robot.sites,
                    "site_mapping": self.robot.site_mapping,
                    "age": self.robot.age,
                    "fullness": self.robot.fullness,
                }
            }
            if self.capture_views:
                self.capture()

            if self.save_results:
                self.save_state(data_obj)

            self.robot_pub.send_pyobj(data_obj)
            # await asyncio.sleep(1 / self.SIMULATE_HZ)
            try:
                env_data = self.env_queue.get(block=False)
            except Empty:
                env_data = None
            try:
                self.robot.step(env_data)
            except Exception as e:
                ic(e)
                self.terminate()

    async def env_subscriber(self):
        poller = zmq.asyncio.Poller()
        poller.register(self.env_sub, zmq.POLLIN)
        while not self.terminate_flag:
            socks = dict(await poller.poll(timeout=100))
            if self.env_sub in socks:
                try:
                    env_data = await self.env_sub.recv_pyobj(zmq.NOBLOCK)
                    self.env_queue.put(env_data)
                except zmq.error.ZMQError as e:
                    ic(e)
                    pass
            else:
                pass

    def terminate(self):
        self.terminate_flag = True
        if self.env_thread.is_alive():
            self.env_thread.join()

        if self.client is not None:
            asyncio.run(self.client.aclose())

        self.env_sub.close()
        self.robot_pub.close()
        self.context.term()

        gc.collect()

    def capture(self):
        rgb_egocentric = self.robot.vision["egocentric"]
        rgb_tracking = self.robot.camera_view["tracking_view"]
        rgb_side = self.robot.camera_view["side_view"]
        # depth_view = self.robot.vision["left_eye"]["depth"]

        cv2.imshow(
            self.egocentric_view_window_name,
            cv2.cvtColor(rgb_egocentric, cv2.COLOR_BGR2RGB),
        )
        cv2.imshow(
            self.tracking_view_window_name,
            cv2.cvtColor(rgb_tracking, cv2.COLOR_BGR2RGB),
        )
        cv2.imshow(
            self.side_view_window_name, cv2.cvtColor(rgb_side, cv2.COLOR_BGR2RGB)
        )
        # cv2.imshow(
        #     self.depth_view_window_name, cv2.cvtColor(depth_view, cv2.COLOR_BGR2RGB)
        # )
        cv2.waitKey(1)

    def save_state(self, data_obj):
        qpos = data_obj[self.robot.name]["qpos"]
        joint_names = [None] * len(qpos)
        for jt_name, jt_state in data_obj[self.robot.name]["joint_states"].items():
            joint_names[jt_state.qpos_adr] = [jt_name] * len(qpos[jt_state.qpos_adr])
        with open(os.path.join(self.save_dir, "qpos.csv"), "a") as f:
            f.write(f"{time.time()}, {qpos}, {joint_names}\n")

    async def close(self):
        await self.client.aclose()


if __name__ == "__main__":
    from argparse import ArgumentParser

    parser = ArgumentParser()
    parser.add_argument("--config", type=str, default="aji6.yaml")
    parser.add_argument(
        "--computer_server_URL", type=str, default="http://localhost:8301"
    )
    parser.add_argument("-capture", "--capture", action="store_true")
    parser.add_argument("-save", "--save_results", action="store_true")
    parser.add_argument("--robot_pub_port", type=int, default=5556)
    parser.add_argument("--env_sub_port", type=int, default=5557)

    args = parser.parse_args()
    cfg = yaml.load(open(args.config, "r"), Loader=yaml.FullLoader)
    args.xml_path = cfg["robot"]["xml_path"]
    args.keyframe = cfg["robot"]["pose"]

    simulator = Aji6Simulator(
        config=cfg,
        xml_path=args.xml_path,
        keyframe=args.keyframe,
        robot_pub_port=args.robot_pub_port,
        env_sub_port=args.env_sub_port,
        capture=args.capture,
        save_results=args.save_results,
    )

    try:
        asyncio.run(simulator.run())
    except KeyboardInterrupt:
        simulator.terminate()
