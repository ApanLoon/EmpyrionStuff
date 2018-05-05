#!/bin/sh

## listAll <path>
##
## Runs the epb program on all files under the given directory recursively.
##

EPB=../src/epb/epb/bin/Debug/epb.exe

find "$1" -name "*.epb" -exec sh -c 'echo -e "$1\r" ; "$0" "$1"' "$EPB" {} ';'
