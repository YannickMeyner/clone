FROM nginx:alpine

RUN echo "<!DOCTYPE html><html><head><title>Hello</title></head><body><h1>Hello World</h1></body></html>" > /usr/share/nginx/html/index.html
