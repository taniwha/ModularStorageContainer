MODNAME		:= ModularStorageContainer
KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
MSCGAMEDATA := ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MSCGAMEDATA}/Plugins

RESGEN2	:= resgen2
GMCS	:= gmcs
GIT		:= git
TAR		:= tar
ZIP		:= zip

.PHONY: all clean info install release

#SUBDIRS=Assets GameData
SUBDIRS=Source

DATA		:= \
	License.txt					\
	README.md					\
	$e

all clean:
	@for dir in ${SUBDIRS}; do \
		make -C $$dir $@ || exit 1; \
	done

install:
	@for dir in ${SUBDIRS}; do \
		make -C $$dir $@ || exit 1; \
	done
	mkdir -p ${MSCGAMEDATA}
	cp ${DATA} ${MSCGAMEDATA}

info:
	@echo "${MODNAME} Build Information"
	@echo "    resgen2:  ${RESGEN2}"
	@echo "    gmcs:     ${GMCS}"
	@echo "    git:      ${GIT}"
	@echo "    tar:      ${TAR}"
	@echo "    zip:      ${ZIP}"
	@echo "    KSP Data: ${KSPDIR}"
	@echo "    Plugin:   ${PLUGINDIR}"

release:
	tools/make-release ${MODNAME}
