// Kad aktiviram na dugme ovu component aktivirace se greska koja ce da aktivira ErrorBoundry 
// Ocu da bude prisutna na svakoj stranici aplikacije i zato je u App.tsx stavljam + nisam je stavio u Routes.tsx
import { useState } from "react";

type Props = {}

const BuggyComponent = (props: Props) => {

  const [shouldThrow, setShouldThrow] = useState(false);

  if (shouldThrow) {
    throw new Error("User triggered error!"); // Aktivirace ErrorBoundary
  }

  // Kliknom na ovo dugme aktiviram ErrorBoundary
  return (
    <button onClick={() => setShouldThrow(true)}>
      Click to crash app and test ErrorBoundary
    </button>
  );
};

export default BuggyComponent;

