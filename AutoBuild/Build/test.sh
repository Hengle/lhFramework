#!/bin/sh
echo "start"
(
cat << EOF
[version]
; 主版本号
major=1
; 次版本号
minor=2
; 修订版本号
revised=1
EOF
) > ../Config/version.ini
