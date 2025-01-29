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
            image_bytes = sub_socket.recv()

            # Try to decode as an image using OpenCV
            np_arr = np.frombuffer(image_bytes, np.uint8)
            image = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

            if image is not None:
                cv2.imshow("Unity -> Python [Subscriber]", image)
                cv2.waitKey(1)
            else:
                # If it's not an image, maybe it's a string
                print(f"Received string: {image_bytes.decode('utf-8', errors='replace')}")

        except Exception as e:
            print(f"Python Subscriber Error: {e}")
            break

if __name__ == "__main__":
    run_image_subscriber()