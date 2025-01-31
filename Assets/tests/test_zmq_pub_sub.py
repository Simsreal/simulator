import json

import zmq
import numpy as np
import cv2

def run_image_subscriber():
    context = zmq.Context()
    sub_socket = context.socket(zmq.SUB)
    # Match the address Unity is publishing from (5556).
    sub_socket.connect("tcp://localhost:5556")
    # Subscribe to all topics (empty string).
    sub_socket.setsockopt_string(zmq.SUBSCRIBE, "")

    print("Python Subscriber started, connecting to Unity tcp://localhost:5556")

    while True:
        try:
            # Receive image or string data from Unity
            data = sub_socket.recv()

            # Try to decode as an image using OpenCV
            state = json.loads(data)
            # print(json.loads(state["robot_joint_data"]).keys()) # need.
            # print(json.loads(state["robot_geom_mapping"])) # need.
            print(json.loads(state["robot_joint_mapping"]))
            exit()
            img = np.frombuffer(bytes(state["egocentric_view"]), dtype=np.uint8)
            img = cv2.imdecode(img, cv2.IMREAD_COLOR)
            cv2.imshow("Unity -> Python [Subscriber]", img)
            cv2.waitKey(1)

        except Exception as e:
            print(f"Python Subscriber Error: {e}")
            break

if __name__ == "__main__":
    run_image_subscriber()