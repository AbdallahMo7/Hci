import cv2
import threading

class CameraHandler:
    def __init__(self, camera_index=0):
        """
        Initializes the camera handler.
        :param camera_index: Index of the camera to use (default is 0 for the primary camera).
        """
        self.camera_index = camera_index
        self.cap = cv2.VideoCapture(camera_index)
        self.frame = None
        self.lock = threading.Lock()
        self.running = False

    def start(self):
        """
        Starts the camera capture loop.
        """
        self.running = True
        threading.Thread(target=self._capture_loop, daemon=True).start()

    def stop(self):
        """
        Stops the camera capture loop.
        """
        self.running = False
        if self.cap.isOpened():
            self.cap.release()

    def _capture_loop(self):
        """
        Continuously captures frames from the camera.
        """
        while self.running:
            ret, frame = self.cap.read()
            if ret:
                with self.lock:
                    self.frame = frame

    def get_frame(self):
        """
        Returns the latest captured frame.
        :return: Current frame.
        """
        with self.lock:
            return self.frame

    def set_frame(self, frame):
        """
        Sets a manipulated frame to replace the current frame.
        :param frame: The manipulated frame.
        """
        with self.lock:
            self.frame = frame

if __name__ == "__main__":
    camera = CameraHandler()
    camera.start()

    try:
        while True:
            frame = camera.get_frame()
            if frame is not None:
                # write hello world on the frame
                cv2.putText(frame, "Hello, World!", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2)
                camera.set_frame(frame)
            frame = camera.get_frame()
            
            if frame is not None:
                cv2.imshow("Camera Feed", frame)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break
    finally:
        camera.stop()
        cv2.destroyAllWindows()
