# Modular Storage Container
Modular Storage Container (MSC) is a Kerbal Space Program mod for
managing a part's internal volume. It is meant to be a replacement for
Modular Fuel Tanks (MFT).

## Concepts
The primary concept behind MSC is a part has a fixed internal volume,
but how that volume is used may vary from instance to instance. MFT does
this as well, but is limited to resources. The goal of MSC is to allow
sub-modules to divide up the volume into different container types,
where the container types represent different uses.

Currently, only resource containers have been implemented, and the
volume available to the containers is not yet dynamic, but this is
mainly a user interface limitation.

An example config:

```
MODULE {
	name = ModuleStorageContainer
	availableVolume = 0.783
	Container {
		name = Resource
		volume = 0.783
		LiquidFuel = 70.2, 70.2
		Oxidizer = 85.8, 85.8
	}
}
```

availableVolume is the volume (in cubic meters) available to be divided
between the container types. Each container type is specified by
Container sub-nodes.

Within a Container sub-node, name is the type of container, and volume
is the amount of volume assigned to that container. The sum of all
container volumes should be less than or equal to availableVolume to
maintain physicality, however ModuleStorageContainer does not enforce
this at the config level.

In this example, the container type is Resource with 0.783m^3 (783L)
assigned to it. Within a resource container node, each resource is
specified by its resource name as the value name with the amount of
resource stored and the maximum storable amount:

```
    <resource name> = <amount>, <maximum amount>
```
