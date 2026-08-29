#!/usr/bin/env bash
# ==================================================================================================
# ========================== DeltaPatcherCLI launcher for macOS and Linux ==========================
# ==================================================================================================
#
# ============================================= USAGE ==============================================
#
# 1. Get the following files:
#    - the lang files for the translation you want to install
#    - from here: https://github.com/Lazy-Desman/DeltranslatePatch/releases/tag/latest
#      - sctipts.7z (REQUIRED)
#      - borders.7z (OPTIONAL, only if you want to enable borders)
#    - from here: https://github.com/roberd82/DeltranslatePatch-Optional/releases/tag/latest
#      - quicktale.7z (OPTIONAL, only if you want to make DeltaQuick packs for Android)
#    - from here: https://github.com/iBotPeaches/Apktool/releases/latest
#      - the latest apktool.jar (OPTIONAL, only if you want to make DeltaQuick packs for Android)
#
# 2. In Steam right-click DELTARUNE -> Manage -> Browse local files, then put this file,
#    the executable, and the extracted folders next to the __MACOSX and DELTARUNE.app folders, so
#    that the game folder looks like this:
#    - __MACOSX
#    - DELTARUNE.app
#    - DeltaPatcherCLI.sh
#    - DeltaPatcherCLI
#    - scripts <- copy the contents of quicktale.7z into this folder and let it override everything
#    - lang
#    - borders
#    - apktool.jar <- remove the version number from the file name
# 
# 3. Edit the CONFIGURATION section below, each setting has a description on what it does.
#    (The settings are set to the macOS defaults by default.)
#
# 4. Make sure this file (DeltaPatcherCLI.sh) is executable:
#    - (in terminal) chmod +x DeltaPatcherCLI.sh
#
# 5. Run the script:
#    - (in terminal) ./DeltaPatcherCLI.sh


set -euo pipefail
# Name of the executable (don't change)
EXE_NAME="DeltaPatcherCLI"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
EXE_PATH="${SCRIPT_DIR}/${EXE_NAME}"


# ========================================= CONFIGURATION ==========================================

# Path to the DELTARUNE game installation (required).
GAME_PATH="${SCRIPT_DIR}/DELTARUNE.app"

# Output directory (optional). Where the lang folder will be copied, and where the packs folder will
# be created when creating QuickTale pack files. If left blank defaults to GAME_PATH.
OUTPUT_PATH=""

# Set the target platform of the patching, avalible options:
# - "" (blank): Windows
# - "--droid": QuickTale on Android
# - "--mac": macOS
PLATFORM_FLAG="--mac"

# Set to true from false to enable console-exclusive borders
BORDERS=false

# Set to true from false to make backups of the game files
MAKE_BACKUPS=false

# List chapters you don't want to get patched like this:
# FILES_TO_SKIP=(0 2) skips the menu (0) and Chapter 2, leave blank to patch every chapter
FILES_TO_SKIP=()

# ======================================= CONFIGURATION END ========================================

# Path to the extracted scripts folder (required).
SCRIPTS_PATH="${SCRIPT_DIR}/scripts"

if [[ ! -f "${EXE_PATH}" ]]; then
    echo "Error: could not find '${EXE_NAME}' next to this script (looked in ${SCRIPT_DIR})." >&2
    exit 1
fi

if [[ ! -x "${EXE_PATH}" ]]; then
    echo "'${EXE_NAME}' is not marked executable, attempting to fix..."
    chmod +x "${EXE_PATH}"
fi

if [[ -z "${GAME_PATH}" ]]; then
    echo "Error: GAME_PATH is empty. Edit this script and set GAME_PATH before running." >&2
    exit 1
fi

if [[ -z "${SCRIPTS_PATH}" ]]; then
    echo "Error: SCRIPTS_PATH is empty. Edit this script and set SCRIPTS_PATH before running." >&2
    exit 1
fi

LANG_SRC="${SCRIPT_DIR}/lang"
if [[ -d "${LANG_SRC}" ]]; then
    if [[ -n "${OUTPUT_PATH}" ]]; then
        LANG_DEST="${OUTPUT_PATH}"
    else
        LANG_DEST="${GAME_PATH}"
    fi
 
    echo "Found 'lang' folder, copying its contents to ${LANG_DEST}..."
    mkdir -p "${LANG_DEST}"
    cp -R "${LANG_SRC}/." "${LANG_DEST}/lang"
    echo
fi
 
# Build the argument list.
ARGS=(--game "${GAME_PATH}" --scripts "${SCRIPTS_PATH}")

if [[ -n "${OUTPUT_PATH}" ]]; then
    ARGS+=(--output "${OUTPUT_PATH}")
fi

if [[ -n "${PLATFORM_FLAG}" ]]; then
    ARGS+=("${PLATFORM_FLAG}")
fi

if [[ "${MAKE_BACKUPS}" == true ]]; then
    ARGS+=(--make-backups)
fi

if [[ ${#FILES_TO_SKIP[@]} -gt 0 ]]; then
    files_arg=""
    for chapter in "${FILES_TO_SKIP[@]}"; do
        if [[ -n "${files_arg}" ]]; then
            files_arg="${files_arg},"
        fi
        files_arg="${files_arg}ch${chapter}"
    done
    ARGS+=(--files "${files_arg}")
fi

if [[ "${BORDERS}" == true ]]; then
    ARGS+=(--borders)
fi

echo "Running: ${EXE_PATH} ${ARGS[*]}"
echo

exec "${EXE_PATH}" "${ARGS[@]}"