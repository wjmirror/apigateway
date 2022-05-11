# Introduction 
Api Gateway on-premises, proxy the on-premises application api request to outside Api.

# Getting Started
1.	Clone the code, setup IIS Express, disable the anonymous authentication, enable the windows authentication only. 
2.  When runs in Local IIS as Web Application, enable Windows authentication only. Check the ReverseProxy.Routes section,ajust the Math.Path matches according to the web application virtual path.


# Reference
The api gateway is backed by reverse proxy Yarp. 
https://microsoft.github.io/reverse-proxy/