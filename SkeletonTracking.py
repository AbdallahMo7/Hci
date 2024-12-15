import cv2
import mediapipe as mp
from CameraHandler import CameraHandler


class HandAndSkeletonTracker:
    def __init__(self):
        """
        Initializes the HandAndSkeletonTracker using MediaPipe.
        """
        self.mp_hands = mp.solutions.hands
        self.mp_pose = mp.solutions.pose
        self.mp_drawing = mp.solutions.drawing_utils
        self.hands = self.mp_hands.Hands(static_image_mode=False, max_num_hands=2, min_detection_confidence=0.5)
        self.pose = self.mp_pose.Pose(static_image_mode=False, min_detection_confidence=0.5, min_tracking_confidence=0.5)

    def get_landmarks(self, frame):
        """
        Processes the frame and extracts hand and skeleton landmarks.
        :param frame: The frame to process.
        :return: A dictionary containing hand and skeleton landmarks.
        """
        results = {
            "hands": [],
            "skeleton": None
        }

        # Convert the frame to RGB
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

        # Process hand landmarks
        hand_results = self.hands.process(rgb_frame)
        if hand_results.multi_hand_landmarks:
            for hand_landmarks in hand_results.multi_hand_landmarks:
                results["hands"].append([
                    {"x": lm.x, "y": lm.y, "z": lm.z}
                    for lm in hand_landmarks.landmark
                ])

        # Process pose (skeleton) landmarks
        pose_results = self.pose.process(rgb_frame)
        if pose_results.pose_landmarks:
            results["skeleton"] = [
                {"x": lm.x, "y": lm.y, "z": lm.z, "visibility": lm.visibility}
                for lm in pose_results.pose_landmarks.landmark
            ]

        return results

    def get_finger_tips(self, frame):
        """
        Extracts the coordinates of the pointer finger tip and thumb tip for each hand.
        :param frame: The frame to process.
        :return: A list of dictionaries containing the finger tips for each hand.
        """
        finger_tips = []
        
        # Convert the frame to RGB
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

        # Process hand landmarks
        hand_results = self.hands.process(rgb_frame)
        if hand_results.multi_hand_landmarks:
            for hand_landmarks in hand_results.multi_hand_landmarks:
                pointer_tip = hand_landmarks.landmark[self.mp_hands.HandLandmark.INDEX_FINGER_TIP]
                thumb_tip = hand_landmarks.landmark[self.mp_hands.HandLandmark.THUMB_TIP]
                finger_tips.append({
                    "pointer_tip": {"x": pointer_tip.x, "y": pointer_tip.y, "z": pointer_tip.z},
                    "thumb_tip": {"x": thumb_tip.x, "y": thumb_tip.y, "z": thumb_tip.z}
                })

                return finger_tips

    def annotate_frame(self, frame):
        """
        Annotates the frame with hand and skeleton landmarks.
        :param frame: The frame to annotate.
        :return: The annotated frame.
        """
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

        # Process hand landmarks and draw them
        hand_results = self.hands.process(rgb_frame)
        if hand_results.multi_hand_landmarks:
            for hand_landmarks in hand_results.multi_hand_landmarks:
                self.mp_drawing.draw_landmarks(
                    frame, hand_landmarks, self.mp_hands.HAND_CONNECTIONS
                )

        # Process pose landmarks and draw them
        pose_results = self.pose.process(rgb_frame)
        if pose_results.pose_landmarks:
            self.mp_drawing.draw_landmarks(
                frame, pose_results.pose_landmarks, self.mp_pose.POSE_CONNECTIONS
            )

        return frame

if __name__ == "__main__":
    camera = CameraHandler(camera_index=2)
    tracker = HandAndSkeletonTracker()
    camera.start()

    try:
        while True:
            frame = camera.get_frame()
            if frame is not None:
                # Annotate the frame
                annotated_frame = tracker.annotate_frame(frame)

                # Get and print the finger tip coordinates
                finger_tips = tracker.get_finger_tips(frame)
                print(finger_tips)

                # Show the annotated camera feed
                cv2.imshow("Hand and Skeleton Tracker", annotated_frame)

                # Keyboard control
                key = cv2.waitKey(1) & 0xFF
                if key == ord('q'):  # Quit the program
                    break

    finally:
        camera.stop()
        cv2.destroyAllWindows()
