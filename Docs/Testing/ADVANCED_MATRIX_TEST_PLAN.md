# Plan: Advanced Transformation Matrix Visualization and Testing Razor Page

## Objective
Create a single interactive Razor page that visually and numerically tests transformation matrices (rotation, translation, scale, pivot, and combinations) for 3D objects. The page will automate test execution, provide visual feedback, and compare actual vs. expected results for each transformation.

---

## Features & Components

### 1. **Test Scenario Buttons**
- Buttons for each test:
  - Rotation (X, Y, Z, arbitrary angles)
  - Translation
  - Scale
  - Pivot (registration point)
  - Combined transformations
- "Run All" button to execute all tests in sequence.

### 2. **Visual 3D Feedback**
- Render a 3D object (cube, sphere, etc.)
- Show axes, grid, and key points (corners, edges)
- Update visualization live as transformations are applied
- Option to toggle between wireframe/solid view

### 3. **Matrix Comparison Table**
- Display actual transformation matrix from `Transform3`
- Display expected matrix for the test case
- Highlight mismatched elements
- Show pass/fail status for each test

### 4. **Transformed Points Table**
- Show original and transformed coordinates of key points
- Compare actual vs. expected transformed points
- Highlight mismatches

### 5. **Test Scenario Definition**
- Each test scenario includes:
  - Name
  - Parameters (rotation, translation, scale, pivot)
  - Expected matrix
  - Expected transformed points

### 6. **Debug Output**
- Show detailed logs for each transformation step
- Print warnings for unit mismatches or unexpected results

### 7. **User Controls**
- Sliders/input boxes for manual parameter adjustment
- Toggle for degrees/radians input
- Option to randomize parameters for stress testing

---

## Implementation Steps

1. **Design TestScenario class**
   - Holds parameters, expected results, and logic for each test
2. **Build Razor page UI**
   - Buttons, tables, 3D canvas, debug output
3. **Integrate BlazorThreeJS for 3D rendering**
   - Render object and axes
4. **Implement matrix and point comparison logic**
   - Automated pass/fail, highlighting
5. **Add user controls for manual testing**
   - Sliders, toggles, input boxes
6. **Test and refine**
   - Ensure all edge cases and combinations are covered

---

## Example Test Scenarios
- Rotate 90° about X, Y, Z
- Translate by (1, 2, 3)
- Scale by (2, 1, 0.5)
- Pivot at (0.5, 0.5, 0.5)
- Combined: Scale, then rotate, then translate, then pivot

---

## Deliverables
- Single Razor page with all features above
- Markdown documentation for usage and conventions
- Automated and visual validation for all transformation types

---

_Last updated: September 7, 2025_
