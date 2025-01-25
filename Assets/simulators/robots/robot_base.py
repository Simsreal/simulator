from abc import ABC, abstractmethod


class RobotBase(ABC):
    def __init__(self, name: str):
        self.name = name

    @abstractmethod
    def step(self):
        pass

    @property
    @abstractmethod
    def joint_states(self):
        pass
