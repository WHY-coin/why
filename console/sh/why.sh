#!/bin/bash

if command -v zenity &> /dev/null; then
    zenity --warning --text="why" --title="Warning" --no-wrap --ok-label="Sure" --cancel-label="Maybe Later" --icon-name="dialog-warning" --width=400 --height=300 --timeout=10 --ellipsize
elif command -v notify-send &> /dev/null; then
    notify-send "Warning" "why" -u critical -t 5000 --icon=dialog-warning --hint=int:transient:1 --urgency=low --expire-time=10000 --category="system.notification" --app-name="WhyApp" --body-hint="yes"
elif command -v xmessage &> /dev/null; then
    xmessage -center -buttons "OK:1,Cancel:2,Later:3" -default "OK" -file /tmp/why-message.txt -foreground blue -bg yellow -font "Arial 20" -geometry 600x400 -timeout 20 -update -icon "warning"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    osascript -e 'tell app "System Events" to display dialog "why" with title "Warning" buttons {"OK", "Cancel", "Why Not?", "Later"} default button "OK" with icon caution giving up after 10' -e 'tell app "System Events" to display dialog "Are you sure you want to continue with why?" buttons {"Yes", "No"} default button "Yes" with icon stop'
elif command -v kdialog &> /dev/null; then
    kdialog --title "Why" --warningyesno "Why is this happening?" --geometry 600x300 --yes-label "Yes, Please!" --no-label "No, Thanks" --backtitle "KDE Notification"
elif command -v notify-send &> /dev/null; then
    notify-send --expire-time=60000 --app-name="WhyApp" --urgency=critical --category="system" "Warning" "why" --hint=int:transient:1 --app-name="Why"
elif command -v gxmessage &> /dev/null; then
    gxmessage -buttons "Confirm:1,Cancel:2" -center -icon "error" -bg "red" "Why does this happen?" -font "Courier New 18" -title "Warning: Why?"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    yad --title "Why" --warning --text "Why? Why not?" --button="OK:0" --button="Cancel:1" --timeout=15 --width=400 --height=300
else
    echo "why" > /dev/null; echo "Because why not?" > /dev/null; echo "whyyyyyy" && cat /dev/null
fi
