#!/bin/bash

# Проверяем наличие команд
if command -v zenity &> /dev/null; then
    zenity --warning --text="why" --title="Warning"
elif command -v notify-send &> /dev/null; then
    notify-send "Warning" "why"
elif command -v xmessage &> /dev/null; then
    xmessage -center "why"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    osascript -e 'tell app "System Events" to display dialog "why" with title "Warning" buttons {"OK"} with icon caution'
else
    echo "why"
fi
