import logging
import time
import xml.etree.ElementTree as ET
from datetime import datetime
from typing import Dict

import numpy as np
from dm_control.mujoco import Physics
from schema.geoms import DGeom
from schema.joints import DJointState
from schema.sites import DSite
from utilities.mj.camera import Camera
from utilities.mj.contact import mujoco_contact_to_dict
from utilities.mj.geoms import compute_net_force_on_geom
from utilities.mj.joints import get_joint_geoms

from simulators.robots.robot_base import RobotBase

logging.getLogger("absl").setLevel(logging.ERROR)


class Aji6(RobotBase):
    imu_sensors = [
        "torso",
    ]

    def __init__(
        self,
        config: dict,
        xml_path: str,
        keyframe: str,
    ):
        super().__init__(config["name"])
        self.config = config
        self.physics = Physics.from_xml_path(xml_path)
        self.m, self.d = self.physics.model, self.physics.data

        self.keyframes = self.get_keyframes(xml_path)
        if keyframe is not None:
            with self.physics.reset_context():
                self.d.qpos[:] = self.keyframes[keyframe]["qpos"]

        self.egocentric = Camera(
            self.physics,
            cam_name="egocentric",
        )
        self.side_view = Camera(
            self.physics,
            cam_name="side",
        )
        self.tracking_view = Camera(
            self.physics,
            cam_name="tracking",
        )
        self.xml_path = xml_path
        self.xml_string = ET.tostring(
            ET.parse(xml_path).getroot(), encoding="utf-8"
        ).decode("utf-8")
        self.body_geoms = self.get_body_geoms(xml_path)

        self.battery_level = 100  # refill upon every launch
        self.last_meal_time = time.time()
        self.last_fullness = 100

    @property
    def geoms(self) -> Dict[str, DGeom]:
        """
        Gather snapshot information about each geom in the model.
        This includes the global position/orientation, size, friction,
        color (rgba), and any other relevant attributes you want to store.
        """
        geom_dict = {}
        for i in range(self.m.ngeom):
            name = self.m.geom(i).name
            # Get model-level properties.
            geom_type = self.m.geom_type[i]
            size = self.m.geom_size[i].copy()
            friction = self.m.geom_friction[i].copy()
            rgba = self.m.geom_rgba[i].copy()

            # Get global position/orientation from data.
            xpos = self.d.geom_xpos[i].copy()
            xmat = self.d.geom_xmat[i].reshape((3, 3)).copy()
            # If you prefer quaternions, use self.d.geom_xquat instead:
            # xquat = self.d.geom_xquat[i].copy()

            # For completeness, you might also want to retrieve the body this geom belongs to:
            body_id = self.m.geom_bodyid[i]

            # Create your custom DGeom object here. Adapt fields as in your DGeom definition.
            geom_dict[name] = DGeom(
                name=name,
                geom_id=i,
                body_id=body_id,
                geom_type=geom_type,
                size=size,
                friction=friction,
                rgba=rgba,
                xpos=xpos,
                xmat=xmat,
            )

        return geom_dict

    @property
    def geom_mapping(self):
        geom_id_to_name = {}
        geom_name_to_id = {}
        for i in range(self.m.ngeom):
            geom_id_to_name[i] = self.m.geom(i).name
            geom_name_to_id[self.m.geom(i).name] = i
        return {
            "geom_id_to_name": geom_id_to_name,
            "geom_name_to_id": geom_name_to_id,
        }

    @property
    def joint_names(self):
        return [self.m.joint(i).name for i in range(self.m.njnt)]

    @property
    def joint_mapping(self):
        joint_id_to_name = {}
        joint_name_to_id = {}
        for i in range(self.m.njnt):
            joint_id_to_name[i] = self.m.joint(i).name
            joint_name_to_id[self.m.joint(i).name] = i
        return {
            "joint_id_to_name": joint_id_to_name,
            "joint_name_to_id": joint_name_to_id,
        }

    @property
    def site_mapping(self):
        site_id_to_name = {}
        site_name_to_id = {}
        for i in range(self.m.nsite):
            name = self.m.site(i).name
            site_id_to_name[i] = name
            site_name_to_id[name] = i
        return {
            "site_id_to_name": site_id_to_name,
            "site_name_to_id": site_name_to_id,
        }

    @property
    def sites(self) -> Dict[str, DSite]:
        """
        Gather snapshot information about each site in the model.
        This includes the global position/orientation, size, color,
        and any other relevant attributes you want to store.
        """
        site_dict = {}
        for i in range(self.m.nsite):
            name = self.m.site(i).name
            body_id = self.m.site_bodyid[i]
            size = self.m.site_size[i].copy()
            rgba = self.m.site_rgba[i].copy()

            # Get global position/orientation from data
            xpos = self.d.site_xpos[i].copy()
            xmat = self.d.site_xmat[i].reshape((3, 3)).copy()

            site_dict[name] = DSite(
                name=name,
                site_id=i,
                body_id=body_id,
                size=np.array(size),
                rgba=rgba,
                xpos=np.array(xpos),
                xmat=np.array(xmat),
            )

        return site_dict

    @property
    def joint_states(self):
        """
        Return a dict with all joint states, including multi-DOF joints.
        Each entry will have 'position', 'velocity', 'effort', 'xpos'.
        Positions, velocities, and efforts will be arrays if the joint is multi-DOF.
        """
        joint_dict = {}
        names = self.joint_names  # some list of joint names or indices

        for i, name in enumerate(names):
            # Access the i-th joint definition from the model
            joint = self.m.joint(i)
            # This is the joint's ID in MuJoCo's indexing
            j_id = joint.id

            # For multi-DOF joints, these addresses point to slices in global arrays
            qpos_adr = self.m.jnt_qposadr[j_id]  # starting index of position in qpos
            dof_adr = self.m.jnt_dofadr[
                j_id
            ]  # starting index of velocity/effort in qvel, qfrc_actuator

            # Determine how many position DOFs this joint has
            # One strategy: if this is the last joint, you can do len(qpos) - qpos_adr.
            # Otherwise, you can do self.m.jnt_qposadr[j_id+1] - qpos_adr to find how many.
            # For simplicity, we'll do a "look ahead" if j_id < m.njnt-1:
            if j_id < (self.m.njnt - 1):
                next_qpos_adr = self.m.jnt_qposadr[j_id + 1]
            else:
                # If j_id is the final joint, positions go until the end of qpos
                next_qpos_adr = self.d.qpos.shape[0]
            npos = next_qpos_adr - qpos_adr

            # The same logic applies to dofs (velocities, efforts)
            if j_id < (self.m.njnt - 1):
                next_dof_adr = self.m.jnt_dofadr[j_id + 1]
            else:
                next_dof_adr = self.d.qvel.shape[0]
            ndof = next_dof_adr - dof_adr

            # Now slice out the correct sections of qpos, qvel, qfrc_actuator
            qpos = self.d.qpos[qpos_adr : qpos_adr + npos]
            qvel = self.d.qvel[dof_adr : dof_adr + ndof]
            effort = self.d.qfrc_actuator[dof_adr : dof_adr + ndof]

            # The body ID for the joint (so we can retrieve, for example, world coordinates)
            body_id = joint.bodyid
            xpos = self.d.xpos[body_id].copy()

            child_geoms, parent_geoms = get_joint_geoms(self.m, joint)
            joint_offset = self.m.jnt_pos[j_id]
            # print(name, joint, self.get_body_chain(body_id[0])); input()

            joint_dict[name] = DJointState(
                name=name,
                qpos=qpos,
                qvel=qvel,
                effort=effort,
                qpos_adr=slice(qpos_adr, qpos_adr + npos),
                qvel_adr=slice(dof_adr, dof_adr + ndof),
                effort_adr=slice(dof_adr, dof_adr + ndof),
                xpos=xpos,
                axis=joint.axis,
                offset=joint_offset,
                parent_geoms=parent_geoms,
                child_geoms=child_geoms,
            )
        return joint_dict

    @property
    def imu(self):
        return {
            imu: {
                "acceleration": self.d.sensor(f"{imu}_accel").data,
                "angular_velocity": self.d.sensor(f"{imu}_gyro").data,
            }
            for imu in self.imu_sensors
        }

    @property
    def contact(self):
        return {
            "contact": mujoco_contact_to_dict(self.d.contact, self.d.ncon),
            "ncon": self.d.ncon,
        }

    @property
    def vision(self) -> np.ndarray:
        return {
            "egocentric": self.egocentric.rgb_image,
        }

    @property
    def camera_view(self) -> np.ndarray:
        return {
            "side_view": self.side_view.rgb_image,
            "tracking_view": self.tracking_view.rgb_image,
        }

    @property
    def force(self):
        force_dict = {}
        body_geoms = self.body_geoms
        geom_mapping = self.geom_mapping["geom_name_to_id"]
        for geom_name in body_geoms:
            geom_id = geom_mapping[geom_name]
            (
                total_force_world,
                force_magnitude,
                force_direction,
            ) = compute_net_force_on_geom(
                self.d.ncon,
                self.d.contact,
                self.d.efc_force,
                geom_id,
            )
            force_dict[geom_name] = {
                "total_force_world": total_force_world,
                "force_magnitude": np.array([force_magnitude]),
                "force_direction": force_direction,
            }
        return force_dict

    @property
    def age(self):
        birth_time = datetime.strptime(
            self.config["birth"], "%Y-%m-%d-%H-%M-%S"
        ).timestamp()
        age_in_seconds = (time.time() - birth_time) * self.config[
            "timelapse_multiplier"
        ]
        return round(age_in_seconds / (365 * 24 * 60 * 60), 1)

    @property
    def fullness(self):
        # FAKE! Decay rate options:
        battery_hours = 12
        decay_rate = 1.0 / (battery_hours * 60 * 60)
        time_passed = (time.time() - self.last_meal_time) * self.config[
            "timelapse_multiplier"
        ]
        remaining = max(0.0, self.last_fullness - (decay_rate * time_passed))
        # print(remaining)
        return remaining

    def get_keyframes(self, xml_path):
        root = ET.parse(xml_path).getroot()
        keyframe_section = root.find(".//keyframe")
        if keyframe_section is None:
            return {}

        keyframes = {}
        for key in keyframe_section.findall("key"):
            name = key.get("name")
            qpos_str = key.get("qpos")
            if qpos_str:
                qpos = np.array([float(x) for x in qpos_str.split()])
                keyframes[name] = {
                    "qpos": qpos,
                }
        return keyframes

    def get_body_chain(self, body_id):
        chain = [self.m.body(body_id).name]
        while body_id != 0:  # 0 is world body
            body_id = self.m.body(body_id).parentid
            chain.append(self.m.body(body_id).name if body_id != 0 else "world")
        return list(reversed(chain))

    def get_body_geoms(self, xml_path):
        root = ET.parse(xml_path).getroot()
        worldbody = root.find(".//worldbody")

        def collect_geom_names(body_element):
            geom_names = []
            geom_names.extend(geom.get("name") for geom in body_element.findall("geom"))

            for child_body in body_element.findall("body"):
                geom_names.extend(collect_geom_names(child_body))

            return geom_names

        all_geom_names = []
        for body in worldbody.findall("body"):
            all_geom_names.extend(collect_geom_names(body))

        return all_geom_names

    def step(self, env_data):
        if env_data is None or env_data[self.name] is None:
            return

        effort_cmds = env_data[self.name]
        for i in range(self.m.nu):
            actuator_name = self.m.actuator(i).name
            self.d.ctrl[i] = effort_cmds.get(actuator_name, 0.0)
            # Step physics forward
        self.physics.step()
