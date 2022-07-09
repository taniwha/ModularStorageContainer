export MODNAME		:= ModularStorageContainer
export KSPDIR		:= ${HOME}/ksp/KSP_linux
export MANAGED		:= ${KSPDIR}/KSP_Data/Managed
export GAMEDATA		:= ${KSPDIR}/GameData
export MODGAMEDATA	:= ${GAMEDATA}/${MODNAME}
export PLUGINDIR	:= ${MODGAMEDATA}/Plugins

TARGETS		:= bin/ModularStorageContainer.dll

MSC_FILES := \
    Assembly/AssemblyInfo.cs	\
	Assembly/VersionReport.cs	\
	IStorageContainer.cs		\
	ModuleStorageContainer.cs	\
	Containers/Resource/ContainerResource.cs	\
	Containers/Resource/Tank.cs	\
	UI/ContainerWindow.cs		\
	UI/InfoLine.cs				\
	UI/InputLine.cs				\
	UI/ResourceInfo.cs			\
	UI/ResourceInfoView.cs		\
	UI/ResourcePanel.cs			\
	UI/WindowManager.cs			\
	Utils/PartExt.cs			\
	$e

include KodeUI/KodeUI.inc

DOC_FILES := \
	License.txt \
	README.md

RESGEN2		:= resgen2
CSC			:= csc
CSCFLAGS	:= -highentropyva- -noconfig -nostdlib+ -t:library -optimize -warnaserror -debug
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: version ${TARGETS}

.PHONY: version
version:
	@../tools/git-version.sh

info:
	@echo "${MODNAME} Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    csc:        ${CSC}"
	@echo "    csc flags:  ${CSCFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

SYSTEM := \
	-lib:${MANAGED} \
	-r:${MANAGED}/mscorlib.dll \
	-r:${MANAGED}/System.dll \
	-r:${MANAGED}/System.Core.dll

KSP := \
	-r:Assembly-CSharp.dll \
	-r:Assembly-CSharp-firstpass.dll

UNITY := \
	-r:UnityEngine.dll \
	-r:UnityEngine.UI.dll \
	-r:UnityEngine.UIModule.dll \
	-r:UnityEngine.TextRenderingModule.dll \
	-r:UnityEngine.AnimationModule.dll \
	-r:UnityEngine.CoreModule.dll \
	-r:UnityEngine.PhysicsModule.dll \
	-r:UnityEngine.Physics2DModule.dll \
	-r:UnityEngine.InputLegacyModule.dll \
	-r:UnityEngine.ImageConversionModule.dll \
	-r:UnityEngine.ParticleSystemModule.dll \
	-r:UnityEngine.AudioModule.dll \
	$e

bin/ModularStorageContainer.dll: ${MSC_FILES} ${KodeUI}
	@mkdir -p bin
	${CSC} ${CSCFLAGS} ${SYSTEM} ${KSP} ${UNITY} -out:$@ $^

clean:
	rm -f ${TARGETS} ModularStorageContainer.dox Assembly/AssemblyInfo.cs
	rm -rf docs
	rm -rf bin

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
#	cp ${DOC_FILES} ${MODGAMEDATA}

doc: version
	doxygen ModularStorageContainer.dox

.PHONY: all clean install doc
