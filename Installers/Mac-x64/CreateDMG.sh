#!/usr/bin/env bash

echo
echo -e "\033[104m\033[97m Adding executable flags \033[0m"
echo

cd ../../Release/Mac-x64/TreeViewer.app/Contents/MacOS/

chmod +x TreeViewer TreeViewerCommandLine DebuggerClient

version=$(strings TreeViewer.dll | grep -A3 "Cross-platform software to draw phylogenetic trees" | grep -v "TreeViewer" | tail -n1)

echo -e "\033[104m\033[97m Setting version number $version \033[0m"
echo

sed -i '' "s/@@VersionHere@@/$version/g" ../Info.plist
sed -i '' "s/@@VersionHere@@/$version/g" ../Resources/DebuggerClient.app/Contents/Info.plist

echo -e "\033[104m\033[97m Creating symlinks for DebuggerClient \033[0m"
echo

rm -rf ../Resources/DebuggerClient.app/Contents/MacOS/

mkdir ../Resources/DebuggerClient.app/Contents/MacOS/

for i in *; do
	ln -s "../../../../MacOS/$i" "../Resources/DebuggerClient.app/Contents/MacOS/$i"
done

rm ../Resources/DebuggerClient.app/Contents/MacOS/DebuggerClient

cp "DebuggerClient" "../Resources/DebuggerClient.app/Contents/MacOS/DebuggerClient"

cd ../../../

echo -e "\033[104m\033[97m Signing app \033[0m"
echo

codesign --deep --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" "TreeViewer.app/Contents/MacOS/createdump"

codesign --deep --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" "TreeViewer.app/Contents/MacOS/TreeViewer"

codesign --deep --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" "TreeViewer.app/Contents/MacOS/DebuggerClient"

codesign --deep --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" "TreeViewer.app/Contents/Resources/DebuggerClient.app/Contents/MacOS/DebuggerClient"

codesign --deep --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" "TreeViewer.app/Contents/MacOS/TreeViewerCommandLine"

find TreeViewer.app/ -name "*.dylib" -type f -exec codesign --deep --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" {} \;

codesign --deep --preserve-metadata="identifier,entitlements,requirements,flags,runtime" --force --timestamp --options=runtime --entitlements="TreeViewer.entitlements" --sign "$1" "TreeViewer.app"

codesign --verify -vvv --strict --deep "TreeViewer.app"

echo
echo -e "\033[104m\033[97m Notarizing app \033[0m"
echo

rm -f "TreeViewer.zip"

ditto -ck --rsrc --sequesterRsrc --keepParent "TreeViewer.app" "TreeViewer.zip"

xcrun notarytool submit TreeViewer.zip --apple-id "$2" --password "$3" --team-id "$4" --wait
xcrun stapler staple TreeViewer.app
xcrun stapler validate TreeViewer.app

rm -f "TreeViewer.zip"

cd ../..

echo
echo -e "\033[104m\033[97m Creating DMG \033[0m"
echo

hdiutil create -volname "TreeViewer" -fs HFS+ -size 350m "TreeViewer.rw.dmg"

device=$(hdiutil attach -readwrite -noverify -noautoopen "TreeViewer.rw.dmg" | grep -e "^/dev/" | head -n1 | cut -f 1)

cp -a Release/Mac-x64/TreeViewer.app /Volumes/TreeViewer/TreeViewer.app
cp Licence.rtf /Volumes/TreeViewer/

mkdir /Volumes/TreeViewer/.background/

cp Icons/Installers/DMG/DMGBackground.png /Volumes/TreeViewer/.background/background.png

echo '
tell application "Finder"
 tell disk "TreeViewer"
  open
  set current view of container window to icon view
  set toolbar visible of container window to false
  set statusbar visible of container window to false
  set the bounds of container window to {100, 100, 795, 790}
  set iconViewOptions to the icon view options of container window
  set arrangement of iconViewOptions to not arranged
  set background picture of iconViewOptions to file ".background:background.png"
  set icon size of iconViewOptions to 128
  set position of item "TreeViewer.app" of container window to {140, 260}
  set position of item "Licence.rtf" of container window to {348, 80}
  make new alias file at container window to POSIX file "/Applications" with properties {name:"Applications"}
  set position of item "Applications" of container window to {555, 260}
  close
  open
  update without registering applications
  close
 end tell
end tell' | osascript


cp Icons/Installers/DMG/DMGIcon.icns /Volumes/TreeViewer/.VolumeIcon.icns
SetFile -a C /Volumes/TreeViewer

sync
sync

hdiutil detach ${device}

hdiutil convert "TreeViewer.rw.dmg" -format UDZO -o "TreeViewer.dmg"

rm "TreeViewer.rw.dmg"

echo
echo -e "\033[104m\033[97m Signing DMG \033[0m"
echo

codesign --deep --force --timestamp --sign "$1" "TreeViewer.dmg"

codesign --verify --verbose "TreeViewer.dmg"

echo
echo -e "\033[104m\033[97m Notarizing DMG \033[0m"
echo



echo
echo -e "\033[94mAll done!\033[0m"
echo

xcrun notarytool submit TreeViewer.dmg --apple-id "$2" --password "$3" --team-id "$4" --wait
xcrun stapler staple TreeViewer.dmg
xcrun stapler validate TreeViewer.dmg
