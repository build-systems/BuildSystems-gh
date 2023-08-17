# BuildSystems-gh

version 0.1.0-alpha2

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


## Surface Properties Tool
We feed this parametric model with data from our building component library. For each component in this library there is information about amount of material, the carbon footprint and prices for each square meter of construction.

### Components
<details>
<summary>Build Library</summary>

Input

* componentDatabase (string: a excel table converted to csv and then commas replaced by ";")
* name (enumerator: a list that is generated automatically)

Output

* Component (data tree with material information for one build component)
</details>

<details>
<summary>Assign Component</summary>

Input

* materialDatabase (string: a excel table converted to csv and then commas replaced by ";")
* surfaces (surfaces, list)
* component (strings, data tree: coming from the Build Library)

Output

* Boxes (box, data tree)
* GWP_A1ToA3 (numbers, data tree: KG of CO2 / each material in phases A1 to A3)
* GWP_AToD (numbers, data tree: KG of CO2 / each material in phases A to D)
</details>
