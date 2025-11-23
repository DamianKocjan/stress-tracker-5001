import { useEffect, useState } from "react";

export function useMediaQuery(query: string) {
  const [matches, setMatches] = useState(false);

  useEffect(() => {
    const mediaQuery = window.matchMedia(query);
    if (mediaQuery.matches !== matches) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setMatches(mediaQuery.matches);
    }

    const handleChange = (event: MediaQueryListEvent) => {
      if (event.matches !== matches) {
        setMatches(event.matches);
      }
    };

    mediaQuery.addEventListener("change", handleChange);
    return () => {
      mediaQuery.removeEventListener("change", handleChange);
    };
  }, [matches, query]);

  return matches;
}
