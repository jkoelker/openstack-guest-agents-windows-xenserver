#!/bin/sh

aclocal
libtoolize -c
automake -c --add-missing
autoconf

