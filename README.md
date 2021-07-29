# Maze Unity

### Edit MazeUnity
* Download git
* Open with unity
* Remove default scene
* Drag and drop main scene from Scenes in the Hierarchy

#### Add Rider in unity (Recommended for editting)
https://www.jetbrains.com/help/rider/Unity.html#getting-started

### Deploy MazeUnity and start webgl server with docker
* Build Settings -> web_build -> (switch platform) -> build
* Choose web_build and name it “webgl”
* Go to to MazeUnity/web_build
        
        cd MazeUnity/web_build
* Edit webgl.conf 'server_name' to the name of your server without the ‘http://’ header (default: localhost)
* Make sure the ports used below are open.
*       docker build -t <image_name>:<version> . (docker build -t maze-unity:1.0.0 .)
*       docker run -p <host_port>:80 <image_name>:<image_version> (docker run -p 12000:80 maze-unity:1.0.0)
* Open <server_name>:<host_port> (localhost:12000) in a browser.
* If you want to stop the docker: `docker stop maze`
* If you want to remove the docker: `docker rm maze`
