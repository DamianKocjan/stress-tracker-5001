"use client";

import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { Range, Root, Thumb, Track } from "@radix-ui/react-slider";
import Color, { type ColorLike } from "color";
import {
  type ComponentProps,
  type HTMLAttributes,
  memo,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { Button } from "./button";

export type ColorPickerProps = HTMLAttributes<HTMLDivElement> & {
  value?: ColorLike;
  defaultValue?: ColorLike;
  onChange?: (value: ColorLike) => void;
  children?: React.ReactNode;
};

export const ColorPicker = ({
  value,
  defaultValue = "#000000",
  onChange,
  className,
  children,
  ...props
}: ColorPickerProps) => {
  const selectedColor = Color(value);
  const defaultColor = Color(defaultValue);

  const [hue, setHue] = useState(
    selectedColor.hue() || defaultColor.hue() || 0
  );
  const [saturation, setSaturation] = useState(
    selectedColor.saturationl() || defaultColor.saturationl() || 100
  );
  const [lightness, setLightness] = useState(
    selectedColor.lightness() || defaultColor.lightness() || 50
  );
  const [alpha, setAlpha] = useState(
    selectedColor.alpha() * 100 || defaultColor.alpha() * 100
  );

  // Update color when controlled value changes
  useEffect(() => {
    if (value) {
      const color = Color.rgb(value).rgb().object();

      setHue(color.r);
      setSaturation(color.g);
      setLightness(color.b);
      setAlpha(color.a);
    }
  }, [value]);

  // Notify parent of changes
  useEffect(() => {
    if (onChange) {
      const color = Color.hsl(hue, saturation, lightness).alpha(alpha / 100);
      onChange(color.hex());
    }
  }, [hue, saturation, lightness, alpha, onChange]);

  return (
    <div className={cn("flex size-full flex-col gap-4", className)} {...props}>
      {children}
    </div>
  );
};

export type ColorPickerSelectionProps = HTMLAttributes<HTMLDivElement> & {
  hue: number;
  setSaturation: (saturation: number) => void;
  setLightness: (lightness: number) => void;
};

export const ColorPickerSelection = memo(
  ({
    className,
    hue,
    setSaturation,
    setLightness,
    ...props
  }: ColorPickerSelectionProps) => {
    const containerRef = useRef<HTMLDivElement>(null);
    const [isDragging, setIsDragging] = useState(false);
    const [positionX, setPositionX] = useState(0);
    const [positionY, setPositionY] = useState(0);

    const backgroundGradient = useMemo(() => {
      return `linear-gradient(0deg, rgba(0,0,0,1), rgba(0,0,0,0)),
            linear-gradient(90deg, rgba(255,255,255,1), rgba(255,255,255,0)),
            hsl(${hue}, 100%, 50%)`;
    }, [hue]);

    const handlePointerMove = useCallback(
      (event: PointerEvent) => {
        if (!(isDragging && containerRef.current)) {
          return;
        }
        const rect = containerRef.current.getBoundingClientRect();
        const x = Math.max(
          0,
          Math.min(1, (event.clientX - rect.left) / rect.width)
        );
        const y = Math.max(
          0,
          Math.min(1, (event.clientY - rect.top) / rect.height)
        );
        setPositionX(x);
        setPositionY(y);
        setSaturation(x * 100);
        const topLightness = x < 0.01 ? 100 : 50 + 50 * (1 - x);
        const lightness = topLightness * (1 - y);

        setLightness(lightness);
      },
      [isDragging, setSaturation, setLightness]
    );

    useEffect(() => {
      const handlePointerUp = () => setIsDragging(false);

      if (isDragging) {
        window.addEventListener("pointermove", handlePointerMove);
        window.addEventListener("pointerup", handlePointerUp);
      }

      return () => {
        window.removeEventListener("pointermove", handlePointerMove);
        window.removeEventListener("pointerup", handlePointerUp);
      };
    }, [isDragging, handlePointerMove]);

    return (
      <div
        className={cn("relative size-full cursor-crosshair rounded", className)}
        onPointerDown={(e) => {
          e.preventDefault();
          setIsDragging(true);
          handlePointerMove(e.nativeEvent);
        }}
        ref={containerRef}
        style={{
          background: backgroundGradient,
        }}
        {...props}
      >
        <div
          className="-translate-x-1/2 -translate-y-1/2 pointer-events-none absolute h-4 w-4 rounded-full border-2 border-white"
          style={{
            left: `${positionX * 100}%`,
            top: `${positionY * 100}%`,
            boxShadow: "0 0 0 1px rgba(0,0,0,0.5)",
          }}
        />
      </div>
    );
  }
);

ColorPickerSelection.displayName = "ColorPickerSelection";

export type ColorPickerHueProps = ComponentProps<typeof Root> & {
  hue: number;
  setHue: (hue: number) => void;
};

export const ColorPickerHue = ({
  className,
  hue,
  setHue,
  ...props
}: ColorPickerHueProps) => {
  return (
    <Root
      className={cn("relative flex h-4 w-full touch-none", className)}
      max={360}
      onValueChange={([hue]) => setHue(hue)}
      step={1}
      value={[hue]}
      {...props}
    >
      <Track className="relative my-0.5 h-3 w-full grow rounded-full bg-[linear-gradient(90deg,#FF0000,#FFFF00,#00FF00,#00FFFF,#0000FF,#FF00FF,#FF0000)]">
        <Range className="absolute h-full" />
      </Track>
      <Thumb className="block h-4 w-4 rounded-full border border-primary/50 bg-background shadow transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50" />
    </Root>
  );
};

export type ColorPickerFormatProps = HTMLAttributes<HTMLDivElement> & {
  hue: number;
  saturation: number;
  lightness: number;
  alpha: number;
};

export const ColorPickerFormat = ({
  className,
  hue,
  saturation,
  lightness,
  alpha,
  ...props
}: ColorPickerFormatProps) => {
  const color = Color.hsl(hue, saturation, lightness, alpha / 100);

  const hex = color.hex();

  return (
    <div
      className={cn(
        "-space-x-px relative flex w-full items-center rounded-md shadow-sm",
        className
      )}
      {...props}
    >
      <Input
        className="h-8 bg-secondary px-2 text-xs shadow-none"
        readOnly
        type="text"
        value={hex}
      />
    </div>
  );
};

export type ColorPickerButtonPreviewProps = ComponentProps<typeof Button> & {
  hue: number;
  saturation: number;
  lightness: number;
  alpha: number;
};

export const ColorPickerButtonPreview = ({
  children,
  hue,
  saturation,
  lightness,
  alpha,
  ...props
}: ColorPickerButtonPreviewProps) => {
  const color = Color.hsl(hue, saturation, lightness, alpha / 100);
  const hex = color.hex();

  const luminance = color.luminosity();
  const hoverColor =
    luminance > 0.5 ? color.darken(0.1).hex() : color.lighten(0.1).hex();
  const textColor = luminance > 0.5 ? "#000000" : "#FFFFFF";

  return (
    <Button
      className="bg-(--bg-color) text-(--text-color) hover:bg-(--hover-bg-color) border-(--text-color)"
      style={
        {
          "--bg-color": hex,
          "--hover-bg-color": hoverColor,
          "--text-color": textColor,
        } as React.CSSProperties
      }
      {...props}
    >
      {children}
    </Button>
  );
};
