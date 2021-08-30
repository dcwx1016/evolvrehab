import cv2
import mediapipe as mp
from socket import *
from threading import Thread
# IrisDepth-related pacakges：
import numpy as np
from custom.iris_lm_depth import from_landmarks_to_depth
# Logging-related:
import logging
import time

# Callback function when receiving request from client:
def recvsocket():
    flag = False;
    while True:
        data, addr = server.recvfrom(1024)
        server.sendto(orientationValues.encode("utf-8"), addr)
        if not flag :
            logging.info("first time sending data")
            flag = True
    logging.info("stop sending")

def helper(index, start, end):
    # Round the prediction result to 0.01
    precision = "{0:0.2f}";
    # Produce a vector representing the direction of this bone.
    x = results_holistic.pose_landmarks.landmark[end].x - results_holistic.pose_landmarks.landmark[start].x;
    y = results_holistic.pose_landmarks.landmark[end].y - results_holistic.pose_landmarks.landmark[start].y;
    z = results_holistic.pose_landmarks.landmark[end].z - results_holistic.pose_landmarks.landmark[start].z;
    return str(index) + "," + str(precision.format(x)) + "," + str(precision.format(y)) + "," + str(precision.format(z)) + ";";

def processData():
    if results_holistic.pose_landmarks:
        sendDraft = helper(10,11, 13);# 0-Right SHOULDER -> Right ELBOW
        sendDraft += helper(11, 13, 15); # 1-Right ELBOW -> Right WRIST
        sendDraft += helper(6, 12, 14);  # 2-LEFT SHOULDER -> LEFT ELBOW
        sendDraft += helper(7, 14, 16);  # 3-LEFT ELBOW -> LEFT WRIST
        sendDraft += helper(17, 23, 25); # 4-Right UpperLeg -> Right LowerLeg
        sendDraft += helper(18, 25, 27); # 5-Right LowerLeg -> Right Foot
        sendDraft += helper(13, 24, 26); # 6-Right UpperLeg -> Right LowerLeg
        sendDraft += helper(14, 26, 28); # 7-Right LowerLeg -> Right Foot
        sendDraft += helper(2, 11, 12);  # 8-chest
        sendDraft += helper(12, 15, 19); # 9-Right Hand
        sendDraft += helper(8, 16, 20);  # 10-Left Hand
        sendDraft += helper(19, 29, 31); # 11-Right Foot
        sendDraft += helper(15, 30, 32); # 12=Left Foot
        sendDraft += f"{(smooth_right_depth) / 10:.2f}"; #13-Camera distance to Right eye
        return sendDraft;
    return "";

if __name__ == "__main__":
    #Logging setup:
    logging.basicConfig(filename='./dividedSitting.log',
                        format='[%(asctime)s-%(message)s]', level=logging.INFO,
                        filemode='a', datefmt='%Y-%m-%d%I:%M:%S')

    #Iris-depth configuration:
    points_idx = [33, 133, 362, 263, 61, 291, 199]
    points_idx = list(set(points_idx))
    points_idx.sort()

    left_eye_landmarks_id = np.array([33, 133])
    right_eye_landmarks_id = np.array([362, 263])

    dist_coeff = np.zeros((4, 1))

    YELLOW = (0, 255, 255)
    GREEN = (0, 255, 0)
    BLUE = (255, 0, 0)
    RED = (0, 0, 255)
    SMALL_CIRCLE_SIZE = 1
    LARGE_CIRCLE_SIZE = 2

    frame_height, frame_width = (720, 1280)
    image_size = (frame_width, frame_height)
    focal_length = frame_width

    landmarks = None
    smooth_left_depth = -1
    smooth_right_depth = -1
    smooth_factor = 0.1

    # Server setup:
    server = socket(AF_INET, SOCK_DGRAM)
    server.bind(("127.0.0.1", 7788))
    childthread = Thread(target=recvsocket, args=())  # assign callback function to child thread
    childthread.start()

    # Global variable to store prediction results.
    orientationValues = "";

    # Holistic and face model configuration:
    mp_drawing = mp.solutions.drawing_utils # utilities for drawing on image
    mp_holistic = mp.solutions.holistic
    mp_face_mesh = mp.solutions.face_mesh

    holistic = mp.solutions.holistic.Holistic(
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5);

    face_mesh =  mp_face_mesh.FaceMesh(
        static_image_mode=False,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    )

    # Camera setup:
    cap = cv2.VideoCapture(0)

    while cap.isOpened():
        DEBUG_POINT = time.time();
        # Fetch fram from camera
        success, frame = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            continue

        # Flip the image horizontally for a later selfie-view display, and convert
        # the BGR image to RGB.
        frame = cv2.cvtColor(cv2.flip(frame, 1), cv2.COLOR_BGR2RGB)
        frame.flags.writeable = False;

        # Process image using face model
        DEBUG_POINT1 = time.time();
        results = face_mesh.process(frame);
        DEBUG_POINT2 = time.time();
        logging.info(str("Face:"+"{0:0.3f}".format(DEBUG_POINT2 - DEBUG_POINT1)));

        # Process image using holistic model
        results_holistic = holistic.process(frame)
        DEBUG_POINT3 = time.time();
        logging.info(str("Holistic:" + "{0:0.3f}".format(DEBUG_POINT3 - DEBUG_POINT2)));

        frame.flags.writeable = True
        frame = cv2.cvtColor(frame, cv2.COLOR_RGB2BGR)

        # Process face landmarks and derive rightEye-depth
        multi_face_landmarks = results.multi_face_landmarks
        if multi_face_landmarks:
            face_landmarks = results.multi_face_landmarks[0]
            landmarks = np.array(
                [(lm.x, lm.y, lm.z) for lm in face_landmarks.landmark]
            )
            landmarks = landmarks.T
            #Using only right-eye distance:
            try:
                (
                    right_depth,
                    right_iris_size,
                    right_iris_landmarks,
                    right_eye_contours,
                ) = from_landmarks_to_depth(
                    frame,
                    landmarks[:, right_eye_landmarks_id],
                    image_size,
                    is_right_eye=True,
                    focal_length=focal_length,
                )
                if smooth_right_depth < 0:
                    smooth_right_depth = right_depth
                else:
                    smooth_right_depth = (
                            smooth_right_depth * (1 - smooth_factor)
                            + right_depth * smooth_factor
                        )
            except Exception as e:
                print(str(e))

            DEBUG_POINT4 = time.time();
            logging.info(str("IrisDepth:" + "{0:0.3f}".format(DEBUG_POINT4 - DEBUG_POINT3)));

        # Draw eyes' landmarks on image:
            if landmarks is not None:
                # write depth values into frame
                depth_string = "{:.2f}cm".format(
                    smooth_right_depth / 10
                )
                frame = cv2.putText(
                    frame,
                    depth_string,
                    (50, 50),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    1,
                    GREEN,
                    2,
                    cv2.LINE_AA,
                )

        #Update orientationValues from the results
        orientationValues = processData()

        mp_drawing.draw_landmarks(
            frame, results_holistic.pose_landmarks, mp_holistic.POSE_CONNECTIONS)
        cv2.imshow('MediaPipe HolisticWithIris', frame)

        logging.info(str("Total:" + "{0:0.2f}".format(time.time() - DEBUG_POINT)));

        if cv2.waitKey(5) & 0xFF == 27:
            cv2.destroyAllWindows()
            break

    cap.release()