//USING
using System;


//INTERFACE
/// <summary>
/// Some network elements have to connect to each other. 
/// They can do it not sooner than when all elements (nodes, links) already exist in the network.
/// This interface allows checking if identificable network element have to be connected.
/// Method Connect is always called when network configuration was read and no more elements are going to be added to the network.
/// </summary>
interface IConnectable
{
    void Connect();
}
