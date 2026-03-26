#!/bin/bash
# Start Engram memory server in background
# Run this before starting OpenCode for persistent memory

engram serve &
echo "Engram server started on http://127.0.0.1:7437"
echo "Press Ctrl+C to stop"
wait
