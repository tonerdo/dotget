#!/bin/bash
mkdir /tmp/dotget
version=$(curl https://api.github.com/repos/tonerdo/dotget/releases/latest | grep -Eo "\"tag_name\":\s*\"(.*)\"" | cut -d'"' -f4)

echo "Installing dotGet $version..."
curl -L https://github.com/tonerdo/dotget/releases/download/$version/dotget.$version.zip > /tmp/dotget/dotget.zip
unzip /tmp/dotget/dotget.zip -d /tmp/dotget
mkdir $HOME/.dotget
cp -r /tmp/dotget/dist $HOME/.dotget/dist
mkdir $HOME/.dotget/bin
cp $HOME/.dotget/dist/Runners/dotnet-get.sh $HOME/.dotget/bin/dotnet-get
chmod +x $HOME/.dotget/bin/dotnet-get
rm -rf /tmp/dotget

if grep -q "$HOME/.dotget/bin" ~/.profile
then
    :
else    
    echo "" >> ~/.profile
    echo "export PATH=$PATH:$HOME/.dotget/bin" >> ~/.profile
fi

echo dotGet was installed successfully!