import threading
import time
import cv2
from CameraHandler import CameraHandler
from EmotionAnalysis import EmotionAnalysis
from FaceIdentification import FaceIdentification
from MarkerRecognition import TUIOReceiver
from ProximityDetector import ProximityDetector
from SkeletonTracking import HandAndSkeletonTracker
from SocketServerCommunication import SocketServer

# Main class to coordinate all functionalities
class MainApplication:
    def __init__(self):
        self.socket_server = SocketServer()
        self.camera_handler = CameraHandler(camera_index=2)
        self.emotion_analyzer = EmotionAnalysis()
        self.face_identifier = FaceIdentification()
        self.tuio_receiver = TUIOReceiver()
        self.proximity_detector = ProximityDetector()
        self.skeleton_tracker = HandAndSkeletonTracker()

        self.socket_thread = threading.Thread(target=self.handle_socket_communication, daemon=True)
        self.camera_thread = threading.Thread(target=self.start_camera_feed, daemon=True)

    def start(self):
        self.socket_server.start_server()
        self.camera_handler.start()

        self.socket_thread.start()
        self.camera_thread.start()

        print("Main application is running.")

        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            self.stop()

    def stop(self):
        print("Shutting down...")
        self.socket_server.stop_server()
        self.camera_handler.stop()

    def handle_socket_communication(self):
        while True:
            request = self.socket_server.receive_message()
            if request:
                print(f"Received request: {request}")
                if request == "$EmotionAnalysis$":
                    threading.Thread(target=self.emotion_analysis_loop, daemon=True).start()
                elif request == "$FaceIdentification$Login$":
                    self.handle_face_login()
                elif request.startswith("$FaceIdentification$Register$"):
                    username = request.split("$")[3]
                    self.handle_face_identification_register(username)
                elif request == "$MarkerRecognition$":
                    threading.Thread(target=self.marker_recognition_loop, daemon=True).start()
                elif request == "$ProximityDetector$":
                    self.handle_proximity_detection()
                elif request == "$AirMouse$":
                    threading.Thread(target=self.air_mouse_loop, daemon=True).start()

    def start_camera_feed(self):
        self.camera_handler.start()
        try:
            while True:
                frame = self.camera_handler.get_frame()
                if frame is not None:
                    cv2.imshow("Camera Feed", frame)
                    if cv2.waitKey(1) & 0xFF == ord('q'):
                        break
        finally:
            self.camera_handler.stop()
            cv2.destroyAllWindows()

    def emotion_analysis_loop(self):
        while True:
            frame = self.camera_handler.get_frame()
            if frame is not None:
                self.emotion_analyzer.process_frame(frame)
                updated_frame = self.emotion_analyzer.overlay_emotion(frame)
                self.camera_handler.set_frame(updated_frame)
                emotion = self.emotion_analyzer.get_dominant_emotion()
                if emotion:
                    self.socket_server.send_message(f"$EmotionAnalysis${emotion}$")
            time.sleep(3)

    def handle_face_login(self):
        frame = self.camera_handler.get_frame()
        if frame is not None:
            user = self.face_identifier.login(frame)
            if user:
                self.socket_server.send_message(f"$FaceIdentification$Login${user}$")
            else:
                self.socket_server.send_message("$FaceIdentification$Login$Failed$No user data found$")

    def handle_face_identification_register(self, username):
        frame = self.camera_handler.get_frame()
        if frame is not None:
            existing_user = username in self.face_identifier.face_database
            similar_face = self.face_identifier.login(frame)

            if existing_user:
                self.server.send_message("$FaceIdentification$Register$Failed$User name already exists$")
            elif similar_face:
                self.server.send_message("$FaceIdentification$Register$Failed$We have similar face data please login$")
            else:
                self.face_identifier.register_face(frame, username)
                self.server.send_message(f"$FaceIdentification$Register$Ok${username}$")

    def marker_recognition_loop(self):
        self.tuio_receiver.run_server()
        while True:
            data = self.tuio_receiver.fetch_data()
            if data:
                self.socket_server.send_message(f"$MarkerRecognition${data}$")

    def handle_proximity_detection(self):
        frame = self.camera_handler.get_frame()
        if frame is not None:
            detected, updated_frame = self.proximity_detector.detect_person(frame)
            self.camera_handler.set_frame(updated_frame)
            if detected:
                self.socket_server.send_message("$ProximityDetector$Ok$")
            else:
                time.sleep(2)
                self.socket_server.send_message("$ProximityDetector$None$")

    def air_mouse_loop(self):
        while True:
            frame = self.camera_handler.get_frame()
            if frame is not None:
                finger_tips = self.skeleton_tracker.get_finger_tips(frame)
                if finger_tips:
                    # annotated_frame = self.skeleton_tracker.annotate_frame(frame)
                    # self.camera_handler.set_frame(annotated_frame)
                    self.socket_server.send_message(f"$AirMouse${finger_tips}$")

if __name__ == "__main__":
    app = MainApplication()
    app.start()
