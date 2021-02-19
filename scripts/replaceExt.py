# python批量更换后缀名
import utility as u
import os
import sys

def main():
    ext = sys.argv[1]
    ext_repl = sys.argv[2]
    path = sys.argv[3]
    if ext == None or ext_repl == None or path == None:
        print("no path, no ext or ext replace provided?")
        return
    for root, dirs, files in os.walk(path):
        for file in files:
            portion = os.path.splitext(file)
            if portion[1] == ("." + ext):
                newName = portion[0] + "." + ext_repl
                print("rename : " + file + " --> " + newName)
                os.rename(os.path.join(root, file), os.path.join(root,newName))

if __name__ == '__main__':
    main()