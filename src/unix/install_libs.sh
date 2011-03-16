#!/bin/sh

libdir=$1

if [ "x$libdir" = "x" ] ; then
    exit 0
fi

if [ ! -d libdir ] ; then
    mkdir $libdir
fi

ldd src/nova-agent | awk '$3 ~ "^/" { print $3 }' | while read lib; do
    cp -p $lib $libdir
done

exit 0
