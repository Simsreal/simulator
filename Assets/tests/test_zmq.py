import zmq
import time
import threading

def run_receiver():
    context = zmq.Context()
    socket = context.socket(zmq.PULL)
    socket.bind("tcp://*:5556")  # Receive Unity messages
    
    print("Receiver started on tcp://*:5556")
    
    while True:
        try:
            message = socket.recv_string()
            print(f"Received from Unity: {message}")
        except Exception as e:
            print(f"Receiver Error: {e}")
            break
    
    socket.close()

def run_sender():
    context = zmq.Context()
    socket = context.socket(zmq.PUSH)
    socket.bind("tcp://*:5557")  # Send messages to Unity
    
    print("Sender started on tcp://*:5557")
    
    count = 0
    while True:
        try:
            message = f"Python message #{count}"
            socket.send_string(message)
            print(f"Sent to Unity: {message}")
            count += 1
            time.sleep(1)  # Send message every second
        except Exception as e:
            print(f"Sender Error: {e}")
            break
    
    socket.close()

if __name__ == "__main__":
    # Start receiver and sender in separate threads
    receiver_thread = threading.Thread(target=run_receiver)
    sender_thread = threading.Thread(target=run_sender)
    
    receiver_thread.start()
    sender_thread.start()
    
    try:
        receiver_thread.join()
        sender_thread.join()
    except KeyboardInterrupt:
        print("Shutting down...")