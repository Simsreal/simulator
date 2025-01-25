from abc import ABC, abstractmethod


class Simulator(ABC):
    """
    Run independently from Simsreal.
    """

    def __init__(self, name: str):
        self.name = name

    @abstractmethod
    def run(self):
        pass

    @abstractmethod
    def terminate(self):
        pass
