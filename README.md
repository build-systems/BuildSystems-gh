# BuildSystems-gh

version 0.2.0-alpha

This version is being developed on top of the Build Systems object Model and the BuildSystems database. Each one have their own repository.

Road map:
* Add the following components
    * Create componsite layer
    * Analise composite layer
    * Create Assembly
    * Analise Assembly
    * Create Component
    * Analise Component

(the text bellow may be outdated)
 
## Urban Level Tools
During the initial phase of a development, to identify if a building will be feasible or not,
we rely on simple rules like city legislation or fire protection standards.
This tool helps to quickly look at those indicators while defining the volumetry of the building, thus, identifying what is the maximum area allowed to be constructed for a specific site.

The most recent version was developed in C#, so it does not require any extra plugins.

### Components
<details>
<summary>Building by Height</summary>

Input

* buildingBoundaries (closed curves)
* foundationHeight (numbers)
* firstFloorHeight (numbers)
* upperFloorsHeight (numbers)
* parapetHeight (numbers)
* buildingTotalHeight (numbers)

Output

* Floors (closed curves)
* Volume (brep)
</details>

<details>
<summary>Building by Floor Number</summary>

Input

* buildingBoundaries (closed curves)
* foundationHeight (numbers)
* firstFloorHeight (numbers)
* upperFloorsHeight (numbers)
* parapetHeight (numbers)
* numberFloors (integer)

Output

* Floors (closed curves)
* Volume (brep)
</details>

<details>
<summary>Global Properties</summary>

Input

* terrainBoundary (one closed curve)
* Floors (closed curves from building components)
* Volumes (brep from building components)

Output

* Properties (text with the urban properties)
</details>


## LCA Tools
Our LCA tool - Life Cycle Assessment.

We have a component with our own in-house developed library assembled on top of [Oköbaudat](https://www.oekobaudat.de/).

### Components
<details>
<summary>Build Library</summary>

Input

* Path (string: root folder containing the three sub-folders with JSON libraries).
* Component (string, component name).

Output

* Component (data tree, material information for one build component).
</details>

<details>
<summary>Assign Component</summary>

Input

* Path (string: root folder containing the three sub-folders with JSON libraries).
* Surfaces (surfaces, building surfaces to generate the bateil).
* Component (strings, component layers in the format of GH Data Tree).
* Phase (string, phases to calculate the GWP and PENRT).

Output

* Boxes (box, representation of materials as Boxes.)
* PENRT (number, PENRT calculated using the material volumes from the Boxes [MJ]).
* GWP (number, GWP calculated using the material volumes from the Boxes [kg CO2-eq]).
</details>
