How to set up the ML Agents package
1. Open Anaconda terminal (does not work with Bash terminal)
2. Create virtual environment with Python version 3.9.13
3. Downgrade protobuf to version 3.20.0
4. Install Torch from specific URL

run the following commands
```
cd /path/to/project
conda create <venv name> python==3.9.13
conda activate <venv name>
pip3 install mlagents
pip3 install protobuf==3.20.0
pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```
