PROJECT(Emgu.Util)

SET(CS_FLAGS -doc:${LIBRARY_OUTPUT_PATH}/Emgu.Util.XML)
SET(SRC_UTIL *.cs Properties/*.cs)

ADD_CS_LIBRARY(Emgu.Util "${SRC_UTIL}")

INSTALL(FILES ${LIBRARY_OUTPUT_PATH}/Emgu.Util.dll
${LIBRARY_OUTPUT_PATH}/Emgu.Util.XML DESTINATION bin
COMPONENT binary)

