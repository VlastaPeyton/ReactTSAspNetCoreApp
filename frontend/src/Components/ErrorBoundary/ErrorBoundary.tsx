import React from "react";
import { ErrorBoundary as ReactErrorBoundary, FallbackProps } from "react-error-boundary";

/*ErrorBoundary sluzi da dohvati sve rendering greske (cak i lazy loading) koje se mogu desiti kako nam ne bi applikacija crash, vec da prikaze neki UI u tom trenutku. 
Obzirom da sam pohvatao sve BE i FE greske, ErrorBoundary cu koristi kao wrapper za <App> u App.tsx. 
*/

// Custom fallback UI shown on error
function ErrorFallback({ error, resetErrorBoundary }: FallbackProps) {
// resetErrorBoundary clears the caught error and allows components inside <ErrorBoundary> in App.tsx to re-render again i resets sve states(useState) to default values accross all app, jer <ErrorBoundary> obuhvata ceo <App>
  return (
    <div role="alert" style={{ padding: 20, backgroundColor: "#fee", color: "#900" }}>
      <p>This is ErrorBoundary! Rendering went wrong, jer sigurno nije BE ili FE error posto sam te greske pohvatao:</p>
      <pre style={{ whiteSpace: "pre-wrap" }}>{error.message}</pre>
      <button onClick={resetErrorBoundary} style={{ marginTop: 10 }}>
        Try again
      </button>
    </div>
  );
}

type Props = {
    children: React.ReactNode //jer u App.tsx bice <ErrorBoundary><App/></ErrorBoundary> tj {children} je <App>
} 

// Reusable ErrorBoundary wrapper component
const ErrorBoundary =  ( {children}: Props ) => {
  return (
    <ReactErrorBoundary
      FallbackComponent={ErrorFallback}
      onReset={() => {
        // Optional: reset app state or do something on retry
        // For example, reset a context or reload data
        // window.location.reload(); // Uncomment to reload page on retry
      }}
    >
      {children}
    </ReactErrorBoundary>
  );
};

export default ErrorBoundary;
