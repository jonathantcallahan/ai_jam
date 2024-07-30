How to set up the ML Agents package
1. Open Anaconda terminal (does not work with Bash terminal)
2. Create virtual environment with Python version 3.9.13
3. Downgrade protobuf to version 3.20.0
4. Install Torch from specific URL

run the following commands
```
cd /path/to/project
conda create --name <venv name> python==3.9.13
conda activate <venv name>
pip3 install mlagents
pip3 install protobuf==3.20.0
pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```
Agents
001 - First attempt.
002 - Updated config. Decreasd batch size (128) increased buffer size significantly (10240) incrased hidden units singificantly (128)
003 - Limited raycast distance for shooting, increased negative reward for missed shot. Increased number raycast rays. Added small negative reward at each step in attempt to prevent the agent from getting stuck in patterns. Added walls to raycast detectable tags.
004 - Increased time horizon and reduced the number of enemies that needed to be killed to end an episode. Significantly increased positive rewards.
005 - Added max steps, set to 5000.
006 - Updated spawner to right agent if they are flipped over.
007 - Removed negative rewards, shortened episode.
008 - Added rotation to observer.
009 - Installed onnx to save model. Increased max step, and increased enemies-killed threshold to end episode. Unity crashed part way through training of this model.
010 - Removed observations for rotation and position. Increased raycast stack to three, and reduced number of rays. Increased stacked vectors to eight.
011 - Switched to POCA, added multiple agents to scene.
012 - Switching back to PPO for multi-agent training with individual rewards, until general movement is improved. Adding slight negative reward each step. Significantly increased batch size, buffer size, max steps, and time horizon.
013 - Decreased negative reward from 0.005 to 0.001.
014 - Decreased time horizon from 640 to 128.
015 - Increased hidden units. Decreased time horizon, batch size, and buffer size.
016 - Decreased negative reward to .0001 
017 - Increased buffer size 4x to 40960 to account for multiple agents.
018 - Starting fresh, decreased buffer size. Decreased ray count increased number of stacked rays. This one crashed.
019 - Decreased number of stacked raycasts. Increased the stacked vectors on the behavior parameters to 10. Started from a fresh file rather than from model 010.
020 - Doubled the number of enemies.
021 - Increased batch size, buffer size, and rays per direction.
022 - Same as before but not inhereting from existing model. Crash appeared to be related to changing model parameters.
023 - Add cooldown as an observation. 