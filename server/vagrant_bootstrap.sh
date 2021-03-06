#!/usr/bin/env bash

export DEBIAN_FRONTEND=noninteractive

# Prepare MongoDB repo
apt-key adv --keyserver keyserver.ubuntu.com --recv 7F0CEB10
echo 'deb http://downloads-distro.mongodb.org/repo/debian-sysvinit dist 10gen' | tee /etc/apt/sources.list.d/mongodb.list

# Update the system
aptitude -q update
aptitude -y -q full-upgrade
aptitude -y -q install python-pip python-dev mongodb-org libtool autoconf uuid-dev

#0MQ
mkdir /tmp/zeromq
cd /tmp/zeromq
wget http://download.zeromq.org/zeromq-3.2.4.tar.gz
tar zxvf zeromq-3.2.4.tar.gz
cd zeromq-3.2.4
./configure
make
make install
ldconfig

# Python packages
pip install pyzmq Werkzeug pymongo python-daemon simplejson

# Daemon stuff
useradd --user-group feeder
mkdir -m 711 /var/log/feeder
chown feeder:feeder /var/log/feeder
mkdir -m 711 /var/run/feeder
chown feeder:feeder /var/run/feeder
mkdir -m 711 /usr/share/feeder
chown feeder:feeder /usr/share/feeder

ln -s /vagrant/feeder.py /usr/share/feeder/feeder.py
ln -s /vagrant/feeder /etc/init.d/feeder
