import cv2
import mediapipe as mp
import time
from socket import *
from threading import Thread
import logging

def recvsocket():
    flag = False;
    while True:
        data, addr = server.recvfrom(1024)
        server.sendto(orientationValues.encode("utf-8"), addr)
        if not flag :
            logging.info("after launching unity")
            flag = True
    logging.info("stop sending")

def helper(index, start, end):
    precision = "{0:0.2f}";
    x = results.pose_landmarks.landmark[end].x - results.pose_landmarks.landmark[start].x;
    y = results.pose_landmarks.landmark[end].y - results.pose_landmarks.landmark[start].y;
    z = results.pose_landmarks.landmark[end].z - results.pose_landmarks.landmark[start].z;
    return str(index) + "," + str(precision.format(x)) + "," + str(precision.format(y)) + "," + str(precision.format(z)) + ";";

def processData():
    if results.pose_landmarks:
        sendDraft = helper(10,11, 13);# Right SHOULDER -> Right ELBOW
        sendDraft += helper(11, 13, 15);# Right ELBOW -> Right WRIST
        sendDraft += helper(6, 12, 14);  # LEFT SHOULDER -> LEFT ELBOW
        sendDraft += helper(7, 14, 16);  # LEFT ELBOW -> LEFT WRIST
        sendDraft += helper(17, 23, 25); # Right UpperLeg -> Right LowerLeg
        sendDraft += helper(18, 25, 27);  # Right LowerLeg -> Right Foot
        sendDraft += helper(13, 24, 26);  # Right UpperLeg -> Right LowerLeg
        sendDraft += helper(14, 26, 28);  # Right LowerLeg -> Right Foot
        sendDraft += helper(2, 11, 12);  # chest
        sendDraft += helper(12, 15, 19); #Right
        sendDraft += helper(8, 16, 20);# Left Hand
        sendDraft += helper(19, 29, 31);  # Right Foot
        sendDraft += helper(15, 30, 32);  # Left Foot
        return sendDraft;
    return "";

if __name__ == '__main__':
    logging.basicConfig(filename='./withouIrisStanding.log',
                        format='[%(asctime)s-FPS:%(message)s]', level=logging.INFO,
                        filemode='a', datefmt='%Y-%m-%d%I:%M:%S')

    mp_drawing = mp.solutions.drawing_utils
    mp_holistic = mp.solutions.holistic
    cap = cv2.VideoCapture(0)

    orientationValues = "";

    server = socket(AF_INET, SOCK_DGRAM)
    server.bind(("127.0.0.1", 7788))
    childthread = Thread(target=recvsocket, args=())
    childthread.start()

    holistic =  mp.solutions.holistic.Holistic(
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5);
    while cap.isOpened():
        DEBUG_POINT = time.time();
        success, image = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            # If loading a video, use 'break' instead of 'continue'.
            continue

        # Flip the image horizontally for a later selfie-view display, and convert
        # the BGR image to RGB.
        image = cv2.cvtColor(cv2.flip(image, 1), cv2.COLOR_BGR2RGB)
        # To improve performance, optionally mark the image as not writeable to
        # pass by reference.
        image.flags.writeable = False
        results = holistic.process(image)
        orientationValues = processData()
        # Draw landmark annotation on the image.
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
        mp_drawing.draw_landmarks(
            image, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS)
        cv2.imshow('MediaPipe Holistic', image)
        logging.info(round(1.0 / (time.time() - DEBUG_POINT)));
        if cv2.waitKey(5) & 0xFF == 27:
            cv2.destroyAllWindows()
            break

    cap.release()