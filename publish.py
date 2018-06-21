from __future__ import print_function
import os,shutil

prjdir = "unxml"
version = "1.0.0.0"
def c(s):
    print(">",s)
    err = os.system(s)
    assert not err

def nuke(pth):
    if os.path.isdir(pth):
        shutil.rmtree(pth)

nuke(prjdir + "/bin")
nuke(prjdir + "/obj")

def pack():
    c("dotnet pack -c release /p:Version=%s" % version)

os.chdir(prjdir)
pack()
