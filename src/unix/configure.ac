AC_PREREQ([2.59])
AC_INIT([agent-smith], [0.0.1.0], [chris.behrens@rackspace.com])
AC_CONFIG_SRCDIR([src/nova-agent.c])
AC_CONFIG_HEADERS([include/config.h])

AM_INIT_AUTOMAKE([foreign -Wall -Werror tar-ustar])

AC_PREFIX_DEFAULT([/usr])

AC_ARG_ENABLE([coverage],
  [AS_HELP_STRING([--enable-coverage],
    [Toggle coverage @<:@default=off@:>@])],
  [ac_coverage="$enableval"],
  [ac_coverage="no"])

AS_IF([test "$ac_coverage" = "yes"],
  [
    GCOV_CFLAGS="-fprofile-arcs -ftest-coverage -lgcov -O0"
  ])


AC_PROG_CC
AM_PROG_CC_C_O
AC_PROG_LIBTOOL
AC_CONFIG_MACRO_DIR([m4])

missing() {
    what="$1"
    echo ""
    echo "The $what is missing and is required for building"
    echo ""
}

# Needed for static linking libcrypto, shouldn't hurt to link otherwise
AC_CHECK_LIB([dl], [dlopen])

AC_CHECK_LIB([pthread], [pthread_create], [], [
    missing "the pthread library"
    exit 1
])

AC_CHECK_LIB([m], [log], [], [
    missing "the math (libm) library"
    exit 1
])

AC_CHECK_LIB([util], [forkpty], [], [
    missing "the util library"
    exit 1
])

AC_CHECK_LIB([z], [inflate], [], [
    missing "the zlib library"
    exit 1
])

AC_CHECK_LIB([crypto], [BIO_new], [], [
    missing "the crypto library"
    exit 1
])

AC_CHECK_LIB([ssl], [SSL_read], [], [
    missing "the crypto library"
    exit 1
])

AC_CHECK_LIB([python2.6], [PyRun_String], [], [
    missing "the python2.6 library"
    exit 1
])

SAVE_LIBS=$LIBS

AC_MSG_CHECKING([for libxenstore.a])

#
# Find the path to libxenstore.a
# XENSTORE_LIBPATHS= can be set before running configure to manually
#  specify a path or paths to look.  Otherwise, the defaults are below
#

if test "$XENSTORE_LIBPATHS" == "" ; then
  XENSTORE_LIBPATHS="/lib64 /usr/lib64 /usr/local/lib64 /lib /usr/lib /usr/local/lib"
fi

XENSTORE_LIB=""

for dir in $XENSTORE_LIBPATHS ; do
  LIBS="$SAVE_LIBS ${dir}/libxenstore.a"
  AC_LINK_IFELSE(
   [AC_LANG_PROGRAM([],
     [[char xs_domain_open(); int main() { xs_domain_open(); return 0; }]])],
   [XENSTORE_LIB="$dir"/libxenstore.a])
  if test "$XENSTORE_LIB" != "" ; then
    break;
  fi
done

LIBS=${SAVE_LIBS}

if test "$XENSTORE_LIB" == "" ; then
  AC_MSG_RESULT([not found])
  missing "xenstore static library"
  echo "If you know the path to libxenstore.a, you should set the"
  echo "XENSTORE_LIBPATHS environment variable to the directory containing it."
  echo "Then re-run configure."
  echo ""
  exit 1

else
  AC_MSG_RESULT([yes (${XENSTORE_LIB})])
fi

#AC_MSG_CHECKING([for json-c version of libjson.a])
#
##
## Find the path to libjson.a
## JSON_LIBPATHS= can be set before running configure to manually
##  specify a path or paths to look.  Otherwise, the defaults are below
##
#
#if test "$JSON_LIBPATHS" == "" ; then
#  JSON_LIBPATHS="/lib64 /usr/lib64 /usr/local/lib64 /lib /usr/lib /usr/local/lib"
#fi
#
#JSON_LIB=""
#
#for dir in $JSON_LIBPATHS ; do
#  LIBS="$SAVE_LIBS ${dir}/libjson.a"
#  AC_LINK_IFELSE(
#   [AC_LANG_PROGRAM([],
#     [[char json_tokener_parse(); int main() { json_tokener_parse(); return 0; }]])],
#   [JSON_LIB="$dir"/libjson.a])
#  if test "$JSON_LIB" != "" ; then
#    break;
#  fi
#done
#
#if test "$JSON_LIB" == "" ; then
#  AC_MSG_RESULT([not found])
#  missing "json-c static library"
#  echo "If you know the path to the correct libjson.a, you should set the"
#  echo "JSON_LIBPATHS environment variable to the directory containing it."
#  echo "Then re-run configure."
#  echo ""
#  exit 1
#else
#  AC_MSG_RESULT([yes (${JSON_LIB})])
#fi
       
LIBS=$SAVE_LIBS

AC_MSG_CHECKING([for libcrypto.a])

#
# Find the path to libcrypto.a
# CRYPTO_LIBPATHS= can be set before running configure to manually
#  specify a path or paths to look.  Otherwise, the defaults are below
#

if test "$CRYPTO_LIBPATHS" == "" ; then
  CRYPTO_LIBPATHS="/lib64 /usr/lib64 /usr/local/lib64 /lib /usr/lib /usr/local/lib"
fi

CRYPTO_LIB=""

for dir in $CRYPTO_LIBPATHS ; do
  LIBS="$SAVE_LIBS ${dir}/libcrypto.a"
  AC_LINK_IFELSE(
   [AC_LANG_PROGRAM([],
     [[char BN_new(); int main() { BN_new(); return 0; }]])],
   [CRYPTO_LIB="$dir"/libcrypto.a])
  if test "$CRYPTO_LIB" != "" ; then
    break;
  fi
done

LIBS=${SAVE_LIBS}

if test "$CRYPTO_LIB" == "" ; then
  AC_MSG_RESULT([not found])
  missing "crypto static library from openssl-dev"

  echo "If you know the path to the correct libcrypto.a, you should set the"
  echo "CRYPTO_LIBPATHS environment variable to the directory containing it."
  echo "Then re-run configure.  But for now, we'll try finding the dynamic"
  echo "version."
  echo ""

  AC_CHECK_LIB([crypto], [BN_new], [],
    [
      missing "crypto library from openssl-dev(el)"
      exit 1
    ])

  LIBS=${SAVE_LIBS}
  CRYPTO_LIB="-lcrypto"

else
  AC_MSG_RESULT([yes (${CRYPTO_LIB})])
fi

AC_SUBST([GCOV_CFLAGS])
AC_SUBST([XENSTORE_LIB])
AC_SUBST([JSON_LIB])
AC_SUBST([CRYPTO_LIB])
AC_CONFIG_FILES([Makefile
                 src/Makefile
                 plugins/Makefile
                 plugins/xenstore/Makefile])
AC_OUTPUT

