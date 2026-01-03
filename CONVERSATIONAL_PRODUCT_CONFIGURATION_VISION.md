# Conversational Product Configuration Vision

## Overview

The Conversational Modeler is fundamentally a **product configuration system** that manages hierarchical bills of materials (BOMs), shopping lists, and component assemblies through natural language conversation. This document describes the vision, architecture, and patterns for building configurable models that represent real-world assemblies, recipes, projects, and any other structured collection of components.

## Core Concept: Models as BOMs and Shopping Lists

### Clarification and Application

The knowledge modeling system has always been capable of representing structured, hierarchical information. This document clarifies how to apply that capability to practical, proven product configuration patterns:

- **Models** = Bills of Materials, Shopping Lists, Recipes, Project Plans
- **Components** = Parts, Items, Ingredients, Tasks, Assemblies
- **Parameters** = Specifications, Properties, Costs, Quantities, Constraints
- **Aggregations** = Total Cost, Total Weight, Combined Specifications

This concrete application makes the system's power immediately accessible for real-world use cases.

## Architecture Layers

### 1. Root Model Level (The BOM/List)

The root model serves as the container and aggregator for the entire configuration:

**Input Specifications:**
- Requirements that define the overall design
- Examples:
  - Recipe: "Serves 10 people"
  - Electronics: "Operates -40°C to 85°C, 12V input"
  - Project: "Complete in 6 weeks, budget $50,000"
  - Mechanical: "Support 500kg load, fit in 2m space"

**Computed Aggregations:**
- Rolled-up totals from all components
- Examples:
  - Total cost (sum of all parts/ingredients)
  - Total weight (sum of all component weights)
  - Total quantity needed
  - Lead time (maximum across components)
  - Nutritional totals (for recipes)
  - Project duration (critical path)

**Properties:**
- Model name and description
- Version/revision information
- Author, date, status
- Any metadata relevant to the domain

### 2. Component Level (The Parts/Items)

Components are the building blocks, and can be:

**Simple Components (Leaf Nodes):**
- Individual parts, ingredients, or items
- Have properties but no children
- Examples:
  - "Tomatoes" with quantity, cost, supplier
  - "Ball Bearing SKF-6000" with dimensions, load rating, price
  - "M6 Bolt" with length, material, quantity needed

**Assembly Components (Parent Nodes):**
- Composed of other components
- Aggregate properties from children
- Examples:
  - "Sauce" assembly containing tomatoes, garlic, oil, spices
  - "Case Assembly" containing case, fans, cables, screws
  - "Wheel Assembly" containing tire, rim, spokes, bearings

**Model Instance Components:**
- Complete models instantiated as components within larger models
- The ultimate reusability pattern
- Examples:
  - "Wheel" model (complete BOM) instantiated 4 times in "Car" model
  - "Server" model instantiated 10 times in "Data Center" model
  - "Sauce Recipe" instantiated in multiple meal models

### 3. Component Properties (Dynamic and Contextual)

Each component can have any properties that make sense for its context:

**Aggregatable Properties:**
- Values that roll up to parents: cost, weight, quantity, volume, power, etc.
- Can be summed, averaged, or computed via other formulas

**Metadata Properties:**
- Name, description, category, tags
- SKU, part number, model number
- Version, revision

**Reference Properties:**
- URLs to product pages, datasheets, images
- Supplier information and contact
- Documentation links

**Specification Properties:**
- Dimensions, tolerances, materials
- Performance characteristics (load, temperature, voltage, etc.)
- Compatibility requirements

**All Defined Dynamically:**
- The system doesn't predefine what properties exist
- Properties emerge from the conversation and domain context
- AI agent determines appropriate properties based on what's being modeled

## Key Patterns and Capabilities

### Pattern 0: Semantic Normalization and Mapping

**The Critical AI Advantage:**

Components in a list can come from wildly different sources with inconsistent terminology, yet the AI agent maintains semantic consistency:

**The Challenge:**
- One source says "weight: 2.5 lbs"
- Another says "mass: 1200g"
- Another says "wt: 1.2kg"
- Another provides "shipping weight: 3 pounds"

**The Solution:**
The AI recognizes these are semantically equivalent and:
1. **Maps to consistent parameter**: All become "Weight"
2. **Normalizes units**: Converts all to common unit (e.g., kg)
3. **Enables aggregation**: Can sum across all components correctly
4. **Maintains source data**: Original values preserved if needed

**Examples Across Domains:**

**Cost/Price:**
- "price: $15.50" → Parameter: Cost, Value: units(15.50, 'USD')
- "cost per unit: €12" → Parameter: Cost, Value: units(12, 'EUR') 
- "retail: 1250 yen" → Parameter: Cost, Value: units(1250, 'JPY')
- Result: Can aggregate total cost in any target currency

**Dimensions:**
- "length: 50mm" → Parameter: Length, Value: units(50, 'mm')
- "size: 2 inches" → Parameter: Length, Value: units(2, 'in')
- "L: 5cm" → Parameter: Length, Value: units(5, 'cm')
- Result: Can compare, filter, aggregate in consistent units

**Quantity:**
- "qty: 24" → Parameter: Quantity, Value: 24
- "count: two dozen" → Parameter: Quantity, Value: 24
- "pack of 24" → Parameter: Quantity, Value: 24
- Result: Can calculate total quantities needed

**This capability is transformative** because:
- Real-world data is messy and inconsistent
- Manual normalization is tedious and error-prone
- AI handles vocabulary variations automatically
- FoundryRulesAndUnits handles unit conversions automatically
- Users get clean, aggregatable data without manual cleanup

### Dynamic Schema Evolution

**The Power of Not Planning Ahead:**

Unlike traditional programming where you must define all properties upfront, the KnModel system allows **dynamic schema evolution**:

**Traditional Approach (Rigid):**
```csharp
class Bolt {
    int Quantity;
    decimal UnitPrice;
    double UnitWeight;
    // Must plan ALL properties at design time
    // Adding new properties requires code changes, recompilation
}
```

**KnModel Approach (Dynamic):**
```
Start Simple:
Component: "M6 Bolt"
├── Quantity: 10
├── UnitPrice: units(0.15, 'USD')
└── UnitWeight: units(5, 'g')

Later, Add Properties as Needed:
Component: "M6 Bolt"
├── Quantity: 10
├── UnitPrice: units(0.15, 'USD')
├── UnitWeight: units(5, 'g')
├── Material: "Stainless Steel 316"        ← Added later
├── Supplier: "McMaster-Carr"              ← Added later
├── LeadTime: units(3, 'd')                ← Added later
├── ThreadPitch: units(1.0, 'mm')          ← Added later
└── TensileStrength: units(500, 'MPa')     ← Added later
```

**Benefits:**
- **No Breaking Changes**: Adding properties doesn't affect existing structure
- **Conversational Discovery**: Properties emerge from conversation, not predetermined
- **Heterogeneous Collections**: Different components can have different properties as needed
- **Evolving Requirements**: Model adapts as understanding deepens
- **No Schema Migrations**: No database schema updates, no code recompilation

**Example Scenario:**
```
User: "Create a hardware list with bolts, nuts, washers"
→ System creates components with Quantity, UnitPrice, UnitWeight

User: "Add the material spec for each item"
→ System adds Material property to existing components

User: "Which ones are stainless steel?"
→ System filters based on newly-added Material property

User: "Add supplier and lead time for stainless items"
→ System adds Supplier and LeadTime to filtered components

User: "Calculate longest lead time"
→ System aggregates over newly-added LeadTime properties
```

This is **incredibly powerful** - the model structure evolves naturally with the conversation, unlike traditional programming where all attributes must be planned ahead of time.

### Pattern 1: Hierarchical Composition

Components can be nested arbitrarily deep:

```
Recipe Model: "Pasta Dinner"
├── Specifications
│   ├── Servings: 4 people
│   └── Dietary: Vegetarian
├── Sauce (assembly)
│   ├── Tomatoes: 800g, $2.50
│   ├── Garlic: 4 cloves, $0.50
│   ├── Olive Oil: 50ml, $1.20
│   └── Spices (assembly)
│       ├── Basil: 10g, $0.80
│       └── Oregano: 5g, $0.60
├── Pasta: 400g, $1.50
└── Cheese: 100g, $3.00
Total Cost: $10.10
```

### Pattern 2: Reusable Assemblies and Models

Create once, reuse many times:

```
Car Model
├── Wheel Model (instance 1) - $250 each
│   ├── Tire: Michelin XYZ, $150
│   ├── Rim: Aluminum 18", $80
│   └── Hardware: lugs, valve, $20
├── Wheel Model (instance 2) - $250
├── Wheel Model (instance 3) - $250
└── Wheel Model (instance 4) - $250
Total Wheel Cost: $1,000
```

### Pattern 3: Alternative Parts and Selection

Components can have multiple options that meet the specification:

```
Bearing Component (10mm ID, 22mm OD, 500N load)
├── Specification: 10mm ID, 22mm OD, min 500N load capacity
├── Alternative 1 (SELECTED): SKF #6000
│   ├── Price: $3.50
│   ├── Load Rating: 650N
│   ├── Lead Time: 2 days
│   └── URL: https://skf.com/6000
├── Alternative 2: NSK #6000VV
│   ├── Price: $2.85
│   ├── Load Rating: 600N
│   ├── Lead Time: 5 days
│   └── URL: https://nsk.com/6000VV
└── Alternative 3: Timken #9100PP
    ├── Price: $4.20
    ├── Load Rating: 750N
    ├── Lead Time: 3 days
    └── URL: https://timken.com/9100PP
```

**Operations on Alternatives:**
- Compare: View all options side-by-side
- Select: Switch to different alternative
- Optimize: "Use cheapest", "Use fastest delivery", "Maximize quality"
- Filter: "Show only alternatives under $3", "Show only in-stock items"

### Pattern 4: Cumulative Aggregation

Properties roll up through the hierarchy:

```
PC Build Model
├── Total Cost: $1,847.50 (computed)
├── Total Weight: 12.3kg (computed)
├── Total Power: 450W (computed)
├── Case Assembly: $180, 8.5kg
│   ├── Case: $120, 7.0kg
│   ├── Fans (3x): $45, 1.2kg
│   └── Cables: $15, 0.3kg
├── Motherboard: $250, 1.1kg, 50W
├── CPU: $420, 0.15kg, 125W
├── GPU: $650, 1.8kg, 225W
└── Power Supply: $347.50, 0.85kg, 50W
```

### Pattern 5: Specifications Drive Selection

Components can be defined by requirements rather than specific parts:

**Instead of:** "Add Intel i7-13700K processor"
**Say:** "Need CPU with 8+ cores, 3.5GHz+, <$500"

The system then:
1. Finds parts meeting the specification
2. Presents alternatives
3. Recommends based on optimization goals
4. Allows selection and comparison

### Pattern 6: Scale and Modify

Models can be scaled or modified, with automatic recalculation:

```
Recipe: "Pasta Dinner" (serves 4)
User: "Scale this recipe for 12 people"
Result: All quantities multiplied by 3, costs recalculated

BOM: "Server Rack" (10 servers)
User: "Change to 20 servers"
Result: Component counts doubled, totals recalculated
```

### Pattern 7: Compare Configurations

Create variations and compare:

```
Configuration A: "Budget Build" - Total: $1,200
Configuration B: "Performance Build" - Total: $2,500
Configuration C: "Balanced Build" - Total: $1,800

Compare on: cost, performance, weight, power consumption
Select best based on: priorities and constraints
```

### Pattern 8: External Data Integration

The AI agent can enrich models by reaching out to external systems:

**RAG Systems (Retrieval-Augmented Generation):**
- Query legacy documentation for specifications
- Extract component details from catalogs and datasheets
- Find historical pricing and lead time data
- Retrieve assembly instructions and best practices
- Access design guidelines and standards

**Example:**
```
User: "Add an M6 bolt"
Agent: [Queries RAG] "Found M6 specifications in standards doc"
       → Creates component with: length options, material grades, torque specs
```

**MCP Servers (Model Context Protocol):**
- Live shopping and price lookups (Amazon, Digi-Key, McMaster-Carr, etc.)
- Inventory and availability checks
- Supplier comparison and selection
- Part cross-reference and equivalents
- Real-time shipping cost calculation

**Example:**
```
User: "Find the cheapest 1kΩ resistors in stock"
Agent: [Calls MCP shopping server]
       → Returns alternatives from multiple suppliers
       → Creates components with current prices, stock levels, delivery times
```

**Web Scraping Tools (Dockling, Beautiful Soup, etc.):**
- Extract product information from any web page
- Parse catalog pages, specification sheets, price lists
- Convert unstructured web content into structured components
- Automatically populate parameters from scraped data
- Handle multiple formats and layouts intelligently

**Example:**
```
User: "Add the motors from this McMaster-Carr page" [provides URL]
Agent: [Scrapes page with Dockling]
       → Extracts: model numbers, specs, prices, stock status
       → Creates component for each motor with normalized parameters
       → User can now compare and select
```

**Example:**
```
User: "Compare the ingredients from these three recipe websites"
Agent: [Scrapes all three URLs]
       → Extracts ingredients, quantities, instructions
       → Normalizes measurements (3 tbsp → 45ml, etc.)
       → Creates unified shopping list with aggregated quantities
```

**Integration Flow:**
1. **User specifies need** (conversationally)
2. **Agent recognizes information gap** (needs specs, prices, alternatives)
3. **Agent queries external system** (RAG for specs, MCP for shopping)
4. **Agent integrates results** (creates/updates components with normalized data)
5. **Model reflects current reality** (live prices, real availability, accurate specs)

**Benefits:**
- **Always current**: Prices, availability, and specs stay up-to-date
- **Comprehensive**: Access to vast catalogs without manual data entry
- **Intelligent**: Agent knows when and where to look for information
- **Automated**: User doesn't need to manually search multiple sites
- **Contextual**: Queries are informed by model requirements and constraints

This makes the Conversational Modeler not just a data structure, but an **active assistant** that can research, shop, compare, and configure on behalf of the user.

## Real-World Application Domains

### Manufacturing & Engineering
- Product BOMs with part alternatives
- Assembly instructions with component breakdowns
- Cost estimation and quoting
- Supply chain planning
- Design for manufacturability analysis

### Food & Recipes
- Ingredient lists with costs and sources
- Nutritional totals
- Scaling for different serving sizes
- Dietary alternatives (gluten-free, vegan options)
- Meal planning and grocery lists

### Construction & Projects
- Material takeoffs
- Labor and resource planning
- Cost estimation
- Schedule and timeline aggregation
- Equipment and tool requirements

### Electronics & Systems
- Component selection (resistors, ICs, connectors)
- Power budget calculations
- Thermal analysis
- BOM generation for PCB assembly
- Part lifecycle and obsolescence tracking

### Procurement & Purchasing
- Shopping lists with price comparison
- Supplier selection
- Order planning and batching
- Inventory management
- Cost optimization

## Integration with FoundryRulesAndUnits

The system leverages FoundryRulesAndUnits for:

**Units and Conversions:**
- Dimensional quantities (length, mass, volume, etc.)
- Automatic unit conversion (kg to lbs, mm to inches, etc.)
- Unit compatibility checking

**Pricing and Currency:**
- Cost units (USD, EUR, etc.)
- Currency conversion
- Cost per unit calculations ($/kg, $/meter, etc.)

**Physical Properties:**
- Material properties
- Environmental conditions (temperature, pressure, etc.)
- Performance specifications (power, speed, flow rate, etc.)

**Dimensional Analysis:**
- Formula validation (ensuring units match)
- Unit propagation through calculations
- Type safety for physical quantities

## Future Extensions: Templates and Abstraction

### Concept Templates

Templates provide an abstraction layer between concept and reality:

**Template Definition:**
- Abstract definition of a category of parts
- Example: "Bearing" template defines: ID, OD, load capacity, speed rating, etc.
- Example: "Ingredient" template defines: quantity, unit, cost, supplier, allergens, etc.

**Template Instantiation:**
- Create concrete components from templates
- Fill in specific values
- Maintain template structure and validation rules

**Template Library:**
- Reusable templates for common component types
- Domain-specific template collections
- Industry standard templates (e.g., electronic components, fasteners, etc.)

**Benefits:**
- Consistency across similar components
- Validation and constraints enforcement
- Easier data entry and configuration
- Better comparison between similar items

### Knowledge Model as Ultimate Configurator

The vision is that templates + instances + alternatives = the ultimate knowledge model:

- **Define once** (template)
- **Instantiate many** (components from template)
- **Alternatives automatically** (find parts matching template spec)
- **Configure interactively** (conversation guides selection)
- **Optimize automatically** (best fit for constraints)

This creates a self-organizing, intelligent configuration system that learns and improves over time.

## Conversational Interface Patterns

### Creating Models

**User:** "Create a shopping list for a dinner party for 8 people"
**System:** Creates model with Servings=8 specification, ready for components

**User:** "Build a BOM for a 3-axis CNC router"
**System:** Creates model with specifications (work area, accuracy, power), starts with major assemblies

### Adding Components

**User:** "Add chicken breast, 2kg"
**System:** Creates component with quantity=2kg, prompts for price/supplier if needed

**User:** "We need linear rails, 1000mm travel, medium precision"
**System:** Creates component with specification, searches for alternatives

### Building Hierarchies

**User:** "The X-axis assembly needs rails, bearings, motor, and belt"
**System:** Creates parent "X-axis assembly", adds child components

**User:** "Organize ingredients into appetizers, main course, and dessert"
**System:** Restructures components into logical groupings

### Working with Alternatives

**User:** "Show me cheaper options for the motor"
**System:** Lists alternatives with comparison table

**User:** "Use the NSK bearing instead"
**System:** Switches selection, recalculates totals

**User:** "What if we use all the cheapest options?"
**System:** Optimizes selections, shows new totals and trade-offs

### Aggregation and Analysis

**User:** "What's the total cost?"
**System:** Reports aggregated total from all selected components

**User:** "How much weight are the steel components?"
**System:** Filters to steel, sums weights, reports total

**User:** "Show me the cost breakdown by category"
**System:** Groups by category, shows sub-totals

## Test Suite Requirements

To validate and demonstrate this vision, we need test suites covering:

### Test Scenario Format

Each test consists of:
1. **User Prompt**: What the user types conversationally
2. **Expected Model Structure**: The components, parameters, and hierarchy that should be created
3. **Expected Aggregations**: What totals/calculations should result
4. **Validation Criteria**: How to verify correctness

### Basic Test Cases

#### Test -1: Voltage Divider Circuit (Active Development Example)

**Status**: Currently being debugged - model root created, components need to follow

**User Prompt:**
```
"Design a voltage divider circuit"
```

**Expected Model Structure:**
```
Model: "VoltageDividerCircuit"
├── Specifications
│   ├── InputVoltage: units(12, 'V')          [User specified or default]
│   ├── DesiredOutputVoltage: units(5, 'V')   [User specified or calculated]
│   └── MaxCurrent: units(0.1, 'A')           [For power calculations]
├── Component: "VoltageSource"
│   └── Voltage: units(12, 'V')
├── Component: "Resistor1 (R1)"
│   ├── Resistance: units(1000, 'Ω')          [Calculated from voltage divider formula]
│   ├── Power: units(0.25, 'W')               [Calculated: I²R or V²/R]
│   ├── MinPowerRating: units(0.5, 'W')       [Safety factor: 2x actual power]
│   └── Type: "1/2W Carbon Film"              [Selected based on power requirement]
├── Component: "Resistor2 (R2)"
│   ├── Resistance: units(700, 'Ω')           [Calculated from voltage divider formula]
│   ├── Power: units(0.18, 'W')               [Calculated: I²R or V²/R]
│   ├── MinPowerRating: units(0.5, 'W')       [Safety factor: 2x actual power]
│   └── Type: "1/2W Carbon Film"              [Selected based on power requirement]
└── Calculated: "OutputVoltage"
    ├── Formula: "V_in × (R2 / (R1 + R2))"
    └── Value: units(4.94, 'V')               [Calculated result]
```

**Available Units (FoundryRulesAndUnits):**
- **Voltage**: V, mV, kV (volts family)
- **Resistance**: Ω, kΩ, MΩ (ohms family)
- **Current**: A, mA, μA (amperes family)
- **Power**: W, mW, kW (watts family)

**Dynamic Capabilities Demonstrated:**

1. **Specification-Driven Selection:**
   - Given: InputVoltage = 12V, DesiredOutputVoltage = 5V
   - Calculate: R1 and R2 values using voltage divider formula
   - Formula: V_out = V_in × (R2 / (R1 + R2))

2. **Power Analysis:**
   - Calculate current through circuit: I = V_in / (R1 + R2)
   - Calculate power dissipation in each resistor: P = I²R
   - Determine required resistor power ratings (with safety factor)
   - Prevent component failure (burnout) by proper selection

3. **Component Selection:**
   - Select resistors with appropriate power ratings:
     - 1/8W (0.125W) for low power
     - 1/4W (0.25W) for standard applications
     - 1/2W (0.5W) for moderate power
     - 1W+ for high power applications
   - Consider tolerance, temperature coefficient, etc.

4. **Dynamic Property Evolution:**
   - Start with basic model: just voltage source and resistors
   - Add power calculations when user asks "Will these resistors be safe?"
   - Add component selection when user asks "What wattage resistors do I need?"
   - Add cost/supplier info when user asks "Where can I buy these?"

**Expected Calculations:**
```
Example: 12V input, 5V output desired, 10mA max current

1. Choose R1 + R2 to limit current:
   R_total = V_in / I_max = 12V / 0.01A = 1200Ω

2. Calculate resistor ratio:
   R2 / (R1 + R2) = V_out / V_in = 5V / 12V = 0.4167
   R2 = 0.4167 × 1200Ω = 500Ω
   R1 = 1200Ω - 500Ω = 700Ω

3. Calculate power dissipation:
   I = 12V / 1200Ω = 0.01A = 10mA
   P_R1 = I² × R1 = (0.01)² × 700 = 0.07W
   P_R2 = I² × R2 = (0.01)² × 500 = 0.05W

4. Select resistor ratings with safety factor (2x):
   R1: Need 0.14W minimum → Use 1/4W (0.25W) resistor
   R2: Need 0.10W minimum → Use 1/4W (0.25W) resistor
```

**Current Status (January 3, 2026):**
- ✅ Root model "VoltageDividerCircuit" created successfully
- ❌ Components not yet added to model tree
- **Issue**: Knowledge Modeling Agent generates plan but doesn't execute ModelTech tool calls
- **Next Step**: Update agent prompt to follow through with AddChild and SetParameter calls

**Validation:**
- Model "VoltageDividerCircuit" exists
- All four components present: VoltageSource, R1, R2, OutputVoltage
- Parameters set correctly for each component
- Formulas calculate correctly
- Power ratings include safety margin
- Component types appropriate for power requirements

**Why This Test:**
- Real-world engineering scenario
- Demonstrates dynamic calculation (voltage divider formula)
- Shows specification-driven selection (resistor values from requirements)
- Illustrates safety constraints (power ratings to prevent burnout)
- Uses existing unit families (Voltage, Resistance, Current, Power)
- Perfect debugging example - partially working, clear next steps

#### Test 0: Simple Hardware List (Minimal Complexity)
**User Prompt:**
```
"Here's a list of hardware: 10 M6 bolts at $0.15 each weighing 5g, 10 M6 nuts at $0.08 each weighing 3g, and 20 M6 washers at $0.05 each weighing 1g. Give me the total price and weight."
```

**Expected Model Structure:**
```
Model: "Hardware List"
├── Component: "M6 Bolts"
│   ├── Quantity: 10
│   ├── UnitPrice: units(0.15, 'USD')  [Currency family: USD, EUR, GBP, JPY, etc.]
│   ├── UnitWeight: units(5, 'g')      [Mass family: kg, g, mg, lb, oz]
│   ├── TotalCost: units(1.50, 'USD')
│   └── TotalWeight: units(50, 'g')
├── Component: "M6 Nuts"
│   ├── Quantity: 10
│   ├── UnitPrice: units(0.08, 'USD')
│   ├── UnitWeight: units(3, 'g')
│   ├── TotalCost: units(0.80, 'USD')
│   └── TotalWeight: units(30, 'g')
└── Component: "M6 Washers"
    ├── Quantity: 20
    ├── UnitPrice: units(0.05, 'USD')
    ├── UnitWeight: units(1, 'g')
    ├── TotalCost: units(1.00, 'USD')
    └── TotalWeight: units(20, 'g')
```

**Expected Aggregations:**
- Total Cost: $3.30 USD (1.50 + 0.80 + 1.00)
- Total Weight: 100g or 0.1kg (50 + 30 + 20)

**Available Units (FoundryRulesAndUnits):**
- **Currency**: USD, EUR, GBP, JPY, CNY, CAD, AUD, CHF, INR, MXN, BRL, KRW, SGD, HKD
- **Mass**: kg, g, mg, μg, ng, t (metric tons), lb, oz, u (atomic mass units)
- **Quantity**: count (dimensionless number)

**Validation:**
- Model created with 3 components
- Each component has Quantity, UnitPrice (Currency family), UnitWeight (Mass family)
- Each component calculates TotalCost = Quantity × UnitPrice
- Each component calculates TotalWeight = Quantity × UnitWeight
- Model aggregates: Total Cost = sum of all TotalCost
- Model aggregates: Total Weight = sum of all TotalWeight
- Values match expected (within rounding)

**Why This Test:**
This is the simplest possible product configuration:
- Flat list (no hierarchy)
- Basic arithmetic (quantity × unit values)
- Two simple aggregations (sum of costs, sum of weights)
- Uses only existing unit families (Currency, Mass, Quantity)
- Common real-world scenario
- Easy to verify manually

#### Test 1: Simple Shopping List
**User Prompt:**
```
"Create a shopping list for tacos for 6 people"
```

**Expected Model Structure:**
```
Model: "Taco Shopping List"
├── Specifications
│   └── Servings: 6 people
├── Component: "Taco Shells"
│   ├── Quantity: units(12, 'count')
│   ├── Cost: units(3.50, 'USD')
│   └── Category: "Pantry"
├── Component: "Ground Beef"
│   ├── Quantity: units(1.5, 'lb')
│   ├── Cost: units(7.50, 'USD')
│   └── Category: "Meat"
├── Component: "Cheese"
│   ├── Quantity: units(8, 'oz')
│   ├── Cost: units(4.00, 'USD')
│   └── Category: "Dairy"
├── Component: "Lettuce"
│   ├── Quantity: units(1, 'head')
│   ├── Cost: units(2.00, 'USD')
│   └── Category: "Produce"
└── Component: "Salsa"
    ├── Quantity: units(16, 'oz')
    ├── Cost: units(3.50, 'USD')
    └── Category: "Condiments"
```

**Expected Aggregations:**
- Total Cost: $20.50 USD
- Total Items: 5 components

**Validation:**
- Model created with name containing "taco" or "shopping"
- At least 4-6 ingredient components
- Each component has Quantity and Cost parameters
- Total cost computed and reasonable ($15-$30 range)

#### Test 2: Hierarchical Assembly
**User Prompt:**
```
"Build a simple wooden bookshelf BOM with materials and hardware"
```

**Expected Model Structure:**
```
Model: "Wooden Bookshelf BOM"
├── Specifications
│   ├── Dimensions: "36in H x 24in W x 12in D"
│   └── Material: "Pine"
├── Assembly: "Frame"
│   ├── Component: "Side Boards (2x)"
│   │   ├── Dimensions: units(36, 'in') x units(12, 'in') x units(0.75, 'in')
│   │   ├── Quantity: 2
│   │   ├── Cost: units(15.00, 'USD') per unit
│   │   └── Total: units(30.00, 'USD')
│   ├── Component: "Top/Bottom Boards (2x)"
│   │   ├── Dimensions: units(24, 'in') x units(12, 'in') x units(0.75, 'in')
│   │   ├── Quantity: 2
│   │   ├── Cost: units(12.00, 'USD') per unit
│   │   └── Total: units(24.00, 'USD')
│   └── Component: "Shelves (3x)"
│       ├── Dimensions: units(22.5, 'in') x units(11.25, 'in') x units(0.75, 'in')
│       ├── Quantity: 3
│       ├── Cost: units(10.00, 'USD') per unit
│       └── Total: units(30.00, 'USD')
└── Assembly: "Hardware"
    ├── Component: "Wood Screws"
    │   ├── Size: "2in #8"
    │   ├── Quantity: 48
    │   ├── Cost: units(8.00, 'USD')
    │   └── Category: "Fasteners"
    └── Component: "Wood Glue"
        ├── Quantity: units(8, 'oz')
        ├── Cost: units(5.00, 'USD')
        └── Category: "Adhesives"
```

**Expected Aggregations:**
- Total Cost: $97.00 USD
- Frame Assembly Cost: $84.00 USD
- Hardware Assembly Cost: $13.00 USD
- Total Lumber: 7 pieces

**Validation:**
- Model has hierarchical structure with assemblies
- Frame assembly contains lumber components
- Hardware assembly contains fasteners/adhesives
- Each component has dimensions or specifications
- Costs aggregate correctly through hierarchy

#### Test 3: Scaling Operation
**User Prompt:**
```
"Create a recipe for chocolate chip cookies for 12 cookies, then scale it to 48 cookies"
```

**Expected Model Structure (Initial):**
```
Model: "Chocolate Chip Cookies"
├── Specifications
│   └── Yield: 12 cookies
├── Component: "All-Purpose Flour"
│   ├── Quantity: units(1.5, 'cup')
│   └── Cost: units(0.30, 'USD')
├── Component: "Butter"
│   ├── Quantity: units(0.5, 'cup')
│   └── Cost: units(1.50, 'USD')
├── Component: "Sugar"
│   ├── Quantity: units(0.5, 'cup')
│   └── Cost: units(0.25, 'USD')
├── Component: "Eggs"
│   ├── Quantity: 1
│   └── Cost: units(0.25, 'USD')
└── Component: "Chocolate Chips"
    ├── Quantity: units(1, 'cup')
    └── Cost: units(2.50, 'USD')
```

**Expected Model Structure (After Scaling to 48):**
```
Model: "Chocolate Chip Cookies"
├── Specifications
│   └── Yield: 48 cookies
├── Component: "All-Purpose Flour"
│   ├── Quantity: units(6, 'cup')  ← Scaled 4x
│   └── Cost: units(1.20, 'USD')   ← Scaled 4x
├── Component: "Butter"
│   ├── Quantity: units(2, 'cup')   ← Scaled 4x
│   └── Cost: units(6.00, 'USD')    ← Scaled 4x
[... all other ingredients scaled by 4x ...]
```

**Expected Aggregations:**
- Initial Total Cost: ~$4.80 USD
- Scaled Total Cost: ~$19.20 USD (4x the original)

**Validation:**
- Initial model created with ~12 cookie yield
- All quantities are reasonable for 12 cookies
- After scaling command, yield updates to 48
- All ingredient quantities multiplied by 4
- All costs multiplied by 4
- Total cost is 4x original

#### Test 4: Alternatives and Selection
**User Prompt:**
```
"Build a PC with a CPU that needs at least 8 cores and costs under $400. Show me options."
```

**Expected Model Structure:**
```
Model: "PC Build"
├── Specifications
│   └── Budget: units(400, 'USD')
├── Component: "CPU"
│   ├── Specification
│   │   ├── MinCores: 8
│   │   ├── MaxCost: units(400, 'USD')
│   │   └── Socket: "AM5 or LGA1700"
│   ├── Alternative 1 (SELECTED): "AMD Ryzen 7 7700X"
│   │   ├── Cores: 8
│   │   ├── BaseClock: units(4.5, 'GHz')
│   │   ├── TDP: units(105, 'W')
│   │   ├── Cost: units(299, 'USD')
│   │   └── URL: "https://amd.com/..."
│   ├── Alternative 2: "Intel Core i7-13700"
│   │   ├── Cores: 16 (8P + 8E)
│   │   ├── BaseClock: units(2.1, 'GHz')
│   │   ├── TDP: units(65, 'W')
│   │   ├── Cost: units(349, 'USD')
│   │   └── URL: "https://intel.com/..."
│   └── Alternative 3: "AMD Ryzen 7 5800X"
│       ├── Cores: 8
│       ├── BaseClock: units(3.8, 'GHz')
│       ├── TDP: units(105, 'W')
│       ├── Cost: units(249, 'USD')
│       └── URL: "https://amd.com/..."
```

**Expected Behavior:**
- Component created with specification (8+ cores, <$400)
- Multiple alternatives presented (2-3 options)
- Each alternative has relevant specs (cores, clock, TDP, cost)
- One alternative marked as selected
- User can switch selections conversationally

**Validation:**
- CPU component created with spec parameters
- At least 2 alternatives present
- All alternatives meet specification (8+ cores, <$400)
- Each alternative has core specs: cores, clock, cost
- Selection mechanism works (can switch between alternatives)

#### Test 5: Model Instantiation
**User Prompt:**
```
"Create a wheel model with tire, rim, and hardware. Then create a car model with 4 wheels."
```

**Expected Model Structure (Wheel Model):**
```
Model: "Wheel"
├── Component: "Tire"
│   ├── Size: "205/55R16"
│   ├── Brand: "Michelin"
│   └── Cost: units(150, 'USD')
├── Component: "Rim"
│   ├── Size: "16 inch"
│   ├── Material: "Aluminum Alloy"
│   └── Cost: units(80, 'USD')
└── Component: "Hardware"
    ├── LugNuts: 5
    ├── ValveStem: 1
    └── Cost: units(20, 'USD')
Total Cost per Wheel: units(250, 'USD')
```

**Expected Model Structure (Car Model):**
```
Model: "Car"
├── Instance: "Front Left Wheel" (from Wheel model)
│   └── Cost: units(250, 'USD')
├── Instance: "Front Right Wheel" (from Wheel model)
│   └── Cost: units(250, 'USD')
├── Instance: "Rear Left Wheel" (from Wheel model)
│   └── Cost: units(250, 'USD')
└── Instance: "Rear Right Wheel" (from Wheel model)
    └── Cost: units(250, 'USD')
Total Wheel Cost: units(1000, 'USD')
```

**Validation:**
- First model "Wheel" created with 3+ components
- Wheel model has computed total cost
- Second model "Car" created
- Car contains 4 instances of Wheel model
- Each instance maintains its own total
- Car's wheel cost = 4 × Wheel model cost

#### Test 6: Semantic Normalization
**User Prompt:**
```
"Add these items to the list:
- Tomatoes: 2 lbs at $3.50
- Onions: weight 500g, price $1.20
- Peppers: mass 300 grams, cost 2 dollars"
```

**Expected Model Structure:**
```
Model: "Shopping List"
├── Component: "Tomatoes"
│   ├── Weight: units(2, 'lb')         [as provided]
│   └── Cost: units(3.50, 'USD')       [normalized to Cost param]
├── Component: "Onions"
│   ├── Weight: units(500, 'g')        [normalized to Weight param]
│   └── Cost: units(1.20, 'USD')       [normalized to Cost param]
└── Component: "Peppers"
    ├── Weight: units(300, 'g')        [normalized to Weight param]
    └── Cost: units(2.00, 'USD')       [normalized to Cost param]
```

**Expected Aggregations:**
- Total Weight: Can compute (requires unit conversion)
  - 2 lb = 907g, + 500g + 300g = 1707g = 1.707kg
- Total Cost: $6.70 USD

**Validation:**
- All three components created
- Despite varied terminology (weight/mass), all map to "Weight" parameter
- Despite varied terminology (price/cost), all map to "Cost" parameter
- Units preserved in parameters (lb, g, USD)
- Can aggregate weight (requires conversion)
- Can aggregate cost (already in same currency)

#### Test 7: External Data Integration
**User Prompt:**
```
"Find the cheapest 1kΩ resistors available in quantities of 100+"
```

**Expected Behavior:**
- Agent recognizes need for external shopping lookup
- Queries MCP server or scrapes supplier sites
- Returns 2-4 alternatives from different suppliers

**Expected Model Structure:**
```
Model: "Resistor Purchase"
├── Specifications
│   ├── Resistance: units(1000, 'ohm')
│   ├── MinQuantity: 100
│   └── Optimize: "Lowest Cost"
├── Component: "1kΩ Resistor"
│   ├── Alternative 1 (SELECTED): "Digi-Key P1.0KQTR-ND"
│   │   ├── Resistance: units(1000, 'ohm')
│   │   ├── Tolerance: "1%"
│   │   ├── Power: units(0.25, 'W')
│   │   ├── Quantity: 100
│   │   ├── UnitPrice: units(0.05, 'USD')
│   │   ├── TotalCost: units(5.00, 'USD')
│   │   ├── InStock: true
│   │   └── URL: "https://digikey.com/..."
│   ├── Alternative 2: "Mouser 123-456-789"
│   │   ├── UnitPrice: units(0.06, 'USD')
│   │   ├── TotalCost: units(6.00, 'USD')
│   │   └── [similar specs...]
│   └── Alternative 3: "LCSC C12345"
│       ├── UnitPrice: units(0.04, 'USD')
│       ├── TotalCost: units(4.00, 'USD')
│       └── [similar specs...]
```

**Validation:**
- Model created with resistance specification
- At least 2 alternatives found from external sources
- Each alternative has: unit price, total cost, availability
- Alternatives sorted by cost (cheapest first)
- URLs present for each alternative
- All meet specification (1kΩ, qty 100+)

### Advanced Test Cases

#### Test 8: Complex Aggregation
**User Prompt:**
```
"Build a BOM for a solar power system with 4 panels, inverter, batteries, and mounting. Calculate total cost and total power output."
```

**Expected Aggregations:**
- Total Cost: Sum of all components
- Total Power Output: Sum of panel wattages
- Total Battery Capacity: Sum of battery amp-hours
- Total System Weight: Sum of all component weights

**Validation:**
- Multiple types of aggregation computed correctly
- Power units handled (W, kW)
- Energy units handled (Wh, kWh, Ah)
- Different unit families don't interfere

#### Test 9: Nested Assemblies (4+ levels deep)
**User Prompt:**
```
"Create a BOM for a server rack with 10 servers, each server has 2 power supplies, each power supply has capacitors and transformers"
```

**Expected Hierarchy:**
```
ServerRack
└── Server (x10)
    └── PowerSupply (x2 per server)
        ├── Capacitor (x5 per supply)
        └── Transformer (x1 per supply)
```

**Validation:**
- 4 levels of nesting
- Correct quantity multiplication (10 servers × 2 supplies × 5 caps = 100 capacitors total)
- Aggregation works through all levels

#### Test 10: Multi-Source Scraping
**User Prompt:**
```
"Compare the ingredient lists from these three recipe URLs and create a unified shopping list"
```

**Expected Behavior:**
- Scrapes all three URLs
- Extracts ingredients from each
- Normalizes measurements
- Combines duplicates (aggregates quantities)
- Creates single unified list

### Test Suite Implementation

To execute these tests:

1. **Feed prompt to Knowledge Modeling Agent** (Claude Sonnet 4.5 via ModelTech tools)
2. **Capture resulting model structure** (components, parameters, hierarchy)
3. **Validate against expected structure**:
   - Are expected components present?
   - Do parameters have correct names and units?
   - Is hierarchy correct?
   - Do aggregations compute correctly?
4. **Report differences** between expected and actual
5. **Iterate prompts** to improve agent performance

### Success Criteria for Tests

- **Structure Match**: 80%+ of expected components and parameters present
- **Semantic Correctness**: Parameter names may vary but meanings must match
- **Unit Consistency**: All units properly specified and compatible
- **Aggregation Accuracy**: Computed totals within 5% of expected
- **Performance**: Each test completes in <30 seconds
- **Robustness**: Tests pass consistently (90%+ success rate across runs)

## Basic Operations
- Create model with specifications
- Add simple components with properties
- Create component hierarchies (assemblies)
- Compute aggregations (cost, weight, etc.)
- Scale models (recipe for 4 → 12)

### Alternative Management
- Define component specifications
- Add alternative parts for a component
- Select/switch between alternatives
- Compare alternatives
- Optimize selection based on criteria

### Model Reuse
- Create a complete model (e.g., "Wheel")
- Instantiate it multiple times in parent model
- Verify independent property changes
- Verify aggregation includes all instances

### Complex Scenarios
- Multi-level hierarchy (4+ levels deep)
- Mixed units requiring conversion
- Cross-model dependencies
- Configuration variants and comparison
- Import/export configurations

### Domain-Specific Examples
- Recipe with nutrition totals
- Electronics BOM with power budget
- Construction material takeoff
- Manufacturing BOM with labor
- Shopping list with store locations

## Implementation Roadmap

### Phase 1: Foundation (Current)
- ✅ KnModel hierarchical structure
- ✅ Parameter system with formulas
- ✅ FoundryRulesAndUnits integration
- ✅ Conversational interface
- ✅ Model Explorer visualization

### Phase 2: Product Configuration Basics
- [ ] Update Knowledge Modeling Agent prompts with BOM/configuration patterns
- [ ] Implement aggregation formulas (sum, total, rollup)
- [ ] Add scaling operations (multiply all quantities)
- [ ] Create example models (recipe, shopping list, simple BOM)
- [ ] Test suite for basic operations

### Phase 3: Alternative Management
- [ ] Component alternative structure
- [ ] Selection mechanism
- [ ] Comparison UI
- [ ] Optimization algorithms
- [ ] Test suite for alternatives

### Phase 4: Advanced Features
- [ ] Model instantiation as components
- [ ] Configuration variants
- [ ] Template system
- [ ] Import/export
- [ ] Test suite for complex scenarios

### Phase 5: Domain Libraries
- [ ] Pre-built templates (electronics, fasteners, ingredients, etc.)
- [ ] Domain-specific aggregation functions
- [ ] Industry-standard part libraries
- [ ] Integration with external catalogs (APIs)

## Success Criteria

The vision is realized when:

1. **A user can conversationally build a complete BOM** for a real product
2. **The system automatically aggregates costs, weights, and other properties**
3. **Multiple alternatives can be compared** and selections optimized
4. **Models can be reused** as components in larger models
5. **The resulting configuration can be exported** for use in procurement, manufacturing, etc.
6. **Domain experts recognize** this as a legitimate product configuration tool
7. **It handles real-world complexity** including units, constraints, and dependencies

## Conclusion

By reframing the Conversational Modeler as a product configuration system focused on BOMs, shopping lists, and component assemblies, we create immediate value and concrete use cases. The system leverages proven patterns from decades of product configuration experience, while adding the power of:

- Natural language interaction
- Flexible, dynamic schemas
- Powerful units and formula system
- Modern web visualization
- AI-assisted configuration

This vision transforms abstract "knowledge modeling" into practical "product configuration by conversation" - a tool that can genuinely help people build, price, compare, and optimize real-world assemblies of all kinds.

---

*Document Version: 1.0 - January 3, 2026*
*Next: Develop test suites to validate and demonstrate these patterns*
