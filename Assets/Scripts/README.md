# Unity Scripts

### HTTP Connectivity

The message exchange with the HTTP server is being done with the use of the Agent (Agent.cs) class.

#### Agent.cs

* Upon Awake, the agent tries to get the configuration file send from MazeRL via Maze-Server.
* Upon StartUp, we initialize the different types of responses we handle during the communication with MazeRL
    * `step_request` A step is requested from MazeRL. This is accompanied with an action to be performed by MazeUnity 
    * `training_request` Training is requested. MazeRL sent information about the progress of the training to be displayed by MazeUnity.
    * `step_response` A response to a step request. This includes
      * the observation of the environment
      * if game has finished
      * the running fps
      * the duration of a pause, if game was paused by the user
      * distance from goal
      * a<sup>human<sup/>
      * a<sup>agent<sup/>
    * `reset_response` A response to a reset request
  
#### Note
All messages(requests & responses) are being serialized to json files before exchanged.