KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
MSCGAMEDATA := ${GAMEDATA}/ModularStorageContainer
PLUGINDIR	:= ${MSCGAMEDATA}/Plugins

TARGETS		:= bin/ModularStorageContainer.dll

MSC_FILES := \
    Assembly/AssemblyInfo.cs	\
	Assembly/VersionReport.cs	\
	Containers/Resource/ContainerResource.cs	\
	Containers/Resource/Tank.cs	\
	ContainerWindow.cs			\
	IStorageContainer.cs		\
	ModuleStorageContainer.cs	\
	GUI/ScrollView.cs			\
	Utils/PartExt.cs			\
	Utils/Styles.cs				\
	$e

DOC_FILES := \
	License.txt \
	README.md

RESGEN2		:= resgen2
GMCS		:= mcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: version ${TARGETS}

.PHONY: version
version:
	@../tools/git-version.sh

info:
	@echo "ModularStorageContainer Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

bin/ModularStorageContainer.dll: ${MSC_FILES}
	@mkdir -p bin
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass \
		-r:UnityEngine,UnityEngine.UI \
		-out:$@ $^

clean:
	rm -f ${TARGETS} ModularStorageContainer.dox Assembly/AssemblyInfo.cs
	rm -rf docs
	rm -rf bin

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
#	cp ${DOC_FILES} ${MSCGAMEDATA}

doc: version
	doxygen ModularStorageContainer.dox

.PHONY: all clean install doc
