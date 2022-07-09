MODNAME		:= ModularStorageContainer
KSPDIR		:= ${HOME}/ksp/KSP_linux
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA  := ${GAMEDATA}/${MODNAME}

ICONS :=

UI_IMAGES := \
	ui_background.png	\
	ui_button.png		\
	ui_checkmark.png	\
	ui_dropdown.png		\
	ui_leftturn.png		\
	ui_rightturn.png	\
	ui_tabdesel.png		\
	ui_tabempty.png		\
	ui_tabsel.png		\
	ui_toggle_off.png	\
	ui_toggle_on.png	\
	$e

UI_CFG := \
	DefaultSkin.cfg		\
	$e

all: ${ICONS} ${UI_IMAGES}

info:
	@echo "    KSP Data:   ${KSPDIR}"

icon_button.png: MSC_app.svg
	inkscape --export-width=38 --export-height=38 --export-type=png -o $@ $^

icon_filter_n.png: MSC_filter.svg
	sed -e 's/\#[[:xdigit:]]\{6\}/#000000/g' -e 's/\#[[:xdigit:]]\{3\}/#000/g' $^ | inkscape --export-width=32 --export-height=32 --export-type=png -o $@ /dev/stdin

icon_filter_s.png: MSC_filter.svg
	sed -e 's/\#[[:xdigit:]]\{6\}/#ffffff/g' -e 's/\#[[:xdigit:]]\{3\}/#fff/g' $^ | inkscape --export-width=32 --export-height=32 --export-type=png -o $@ /dev/stdin

ui_background.png: ui_background.svg
	inkscape --export-width=32 --export-height=32 --export-type=png -o $@ $^

ui_button.png: ui_button.svg
	inkscape --export-width=16 --export-height=16 --export-type=png -o $@ $^

ui_checkmark.png: ui_checkmark.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_dropdown.png: ui_dropdown.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_leftturn.png: ui_leftturn.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_rightturn.png: ui_rightturn.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabdesel.png: ui_tabdesel.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabempty.png: ui_tabempty.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabsel.png: ui_tabsel.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_toggle_off.png: ui_toggle_off.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_toggle_on.png: ui_toggle_on.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

clean:
	rm -f ${ICONS} ${FLAGS} ${UI_IMAGES}

install: all
	#mkdir -p ${MODGAMEDATA}/Textures
	#cp icon_button.png icon_filter*.png ${MODGAMEDATA}/Textures
	mkdir -p ${MODGAMEDATA}/UI
	cp ${UI_CFG} ${UI_IMAGES} ${MODGAMEDATA}/UI

.PHONY: all clean install
