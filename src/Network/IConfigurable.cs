//USING

//INTERFACE
/// <summary>
/// After connecting to existing network elements some devices (like StaticRoutingProtocol have to confige themselves.
/// The existing network has to be fully connected to do it.
/// It requires next phase of creating network: network configuration.
/// </summary>
interface IConfigurable
{
    void Configure();
}