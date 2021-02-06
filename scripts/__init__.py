import sys
from importlib import import_module
import imp
from os.path import dirname

sys.path.append(dirname(__file__))
# for item in sys.path:
    # print("Script running path>>>[%s]"%(item))