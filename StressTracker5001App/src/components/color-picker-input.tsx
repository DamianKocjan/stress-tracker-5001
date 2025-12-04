import Color, { type ColorLike } from "color";
import { useEffect, useState } from "react";
import {
  ColorPicker,
  ColorPickerButtonPreview,
  ColorPickerFormat,
  ColorPickerHue,
  ColorPickerSelection,
} from "./ui/color-picker";
import { Popover, PopoverContent, PopoverTrigger } from "./ui/popover";

interface ColorPickerInputProps {
  defaultValue?: string;
  value: string;
  onChange: (value: string) => void;
}

export function ColorPickerInput({
  defaultValue,
  value,
  onChange,
}: ColorPickerInputProps) {
  const selectedColor = Color(value);
  const defaultColor = Color(defaultValue || "#000000");

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
      const color = Color(value);
      setHue(color.hue() || 0);
      setSaturation(color.saturationl() || 100);
      setLightness(color.lightness() || 50);
      setAlpha(color.alpha() * 100);
    }
  }, [value]);

  // Notify parent of changes
  useEffect(() => {
    const color = Color.hsl(hue, saturation, lightness).alpha(alpha / 100);
    onChange(color.hex());
  }, [hue, saturation, lightness, alpha, onChange]);

  return (
    <ColorPicker
      defaultValue={defaultValue}
      value={value}
      onChange={onChange as (color: ColorLike) => void}
    >
      <Popover>
        <PopoverTrigger asChild>
          <ColorPickerButtonPreview
            hue={hue}
            saturation={saturation}
            lightness={lightness}
            alpha={alpha}
          >
            {value ? "Change Color" : "Select Color"}
          </ColorPickerButtonPreview>
        </PopoverTrigger>
        <PopoverContent className="space-y-4">
          <ColorPickerSelection
            className="min-h-12 aspect-square"
            hue={hue}
            setSaturation={setSaturation}
            setLightness={setLightness}
          />
          <ColorPickerHue hue={hue} setHue={setHue} />
          <ColorPickerFormat
            hue={hue}
            saturation={saturation}
            lightness={lightness}
            alpha={alpha}
          />
        </PopoverContent>
      </Popover>
    </ColorPicker>
  );
}
