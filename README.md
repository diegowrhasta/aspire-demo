# Aspire Demo

This is a .NET Aspire DEMO to get in touch with what is possible with .NET Aspire and integrating it with different backing 
services and so on.

So basically to register quadlets you need to work with these two paths:

- `/etc/systemd/system/`
- `/etc/containers/systemd/`

Under `systemd/` you would register all the services you want to run, they are suffixed with `.container` they in turn will 
become `.service` if you want to start/stop/restart manually.

And under `system/` you would then register the "entry-point", which in short is just a declaration of a group of services you 
want to start all at once E.g., `.target`

## Usage

**NOTE:** Remember you need to have all the images already available to the host so that the services can spin them up.

To start the whole "orchestra" you can do just this:

`sudo systemctl start aspire.target`

And if you wanna manipulate individual services:

`sudo systemctl start env-dashboard.service`, and so on.

Don't forget that `podman` is at the core helping with container runtime functions. So you can also check out things that 
are running with: `podman ps` and other commands like it