FROM nginx:alpine

WORKDIR /webgl
COPY web_build/webgl/ .

WORKDIR /etc/nginx/conf.d
RUN rm default.conf
COPY web_build/webgl.conf webgl.conf
