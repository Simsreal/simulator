from utilities.network.retry import async_retry


@async_retry
def lwrist_to_head_ik_task(robot):
    return robot.compute_ik(
        site_name="left_palm_imu",
        target_pos=robot.d.site("head_site").xpos,
        joint_names=[
            "shoulder1_left",
            "shoulder2_left",
            "elbow_left",
        ],
    )


@async_retry
def rwrist_to_head_ik_task(robot):
    return robot.compute_ik(
        site_name="right_palm_imu",
        target_pos=robot.d.site("head_site").xpos,
        joint_names=[
            "shoulder1_right",
            "shoulder2_right",
            "elbow_right",
        ],
    )
