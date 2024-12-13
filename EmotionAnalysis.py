from CameraHandler import CameraHandler
from deepface import DeepFace
import cv2
from collections import Counter

class EmotionAnalysis:
    def __init__(self, camera_handler, window_size=10):
        """
        Initializes the EmotionAnalysis class.
        :param camera_handler: Instance of the CameraHandler class.
        :param window_size: Number of frames to consider for emotion analysis.
        """
        self.camera_handler = camera_handler
        self.window_size = window_size
        self.emotions = []

    def analyze_emotion(self, frame):
        """
        Analyzes the emotion of a single frame using DeepFace.
        :param frame: Frame to analyze.
        :return: Detected emotion (string) or None if detection fails.
        """
        try:
            # DeepFace might return a list or a dictionary; handle both cases
            result = DeepFace.analyze(frame, actions=['emotion'], enforce_detection=False)
            if isinstance(result, list):
                result = result[0]  # If a list is returned, take the first result
            return result.get('dominant_emotion', None)
        except Exception as e:
            print(f"Emotion analysis failed: {e}")
            return None

    def process_frame(self):
        """
        Processes the current frame to detect and store the dominant emotion.
        """
        frame = self.camera_handler.get_frame()
        if frame is not None:
            emotion = self.analyze_emotion(frame)
            if emotion:
                self.emotions.append(emotion)
                # Keep the sliding window size
                if len(self.emotions) > self.window_size:
                    self.emotions.pop(0)

    def get_dominant_emotion(self):
        """
        Calculates the dominant emotion from the sliding window of emotions.
        :return: Dominant emotion (string) or None if no emotions are detected.
        """
        if self.emotions:
            return Counter(self.emotions).most_common(1)[0][0]
        return None

    def overlay_emotion(self, frame):
        """
        Overlays the dominant emotion as text on the frame.
        :param frame: Frame to overlay text on.
        :return: Frame with overlaid emotion text.
        """
        dominant_emotion = self.get_dominant_emotion()
        if dominant_emotion:
            cv2.putText(frame, f"Emotion: {dominant_emotion}", (10, 30),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA)
        return frame

if __name__ == "__main__":
    # Initialize CameraHandler
    camera = CameraHandler()
    camera.start()

    # Initialize EmotionAnalysis
    emotion_analyzer = EmotionAnalysis(camera)

    try:
        while True:
            # Process the current frame
            emotion_analyzer.process_frame()

            # Get the current frame and overlay emotion text
            frame = camera.get_frame()
            if frame is not None:
                frame_with_emotion = emotion_analyzer.overlay_emotion(frame)

                # Display the frame
                cv2.imshow("Emotion Analysis", frame_with_emotion)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break
    finally:
        camera.stop()
