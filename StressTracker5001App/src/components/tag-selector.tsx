import {
  Tags,
  TagsContent,
  TagsEmpty,
  TagsGroup,
  TagsInput,
  TagsItem,
  TagsList,
  TagsTrigger,
  TagsValue,
} from "@/components/ui/tag-selector";
import type { TagDto } from "@/dto/tag.dto";
import { CheckIcon } from "lucide-react";
import { useEffect, useRef, useState } from "react";

interface TagSelectorProps {
  availableTags: TagDto[];
  selectedTags: TagDto[];
  onClose?: (tags: number[]) => void;
  maxTags?: number;
  disabled?: boolean;
}

export function TagSelector({
  availableTags,
  selectedTags: initialSelectedTags,
  onClose,
  maxTags = 5,
  disabled = false,
}: TagSelectorProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedTags, setSelectedTags] = useState<number[]>(
    initialSelectedTags.map((tag) => tag.id)
  );
  const initialValue = useRef(selectedTags);
  const canAddMore = selectedTags.length < maxTags;

  useEffect(() => {
    if (!initialSelectedTags) {
      return;
    }

    // eslint-disable-next-line react-hooks/set-state-in-effect
    setSelectedTags(initialSelectedTags.map((tag) => tag.id));
    initialValue.current = initialSelectedTags.map((tag) => tag.id);
  }, [initialSelectedTags]);

  function handleIsOpenChange() {
    setIsOpen((prev) => {
      const hasChanged =
        initialValue.current.length !== selectedTags.length ||
        initialValue.current.some((tagId) => !selectedTags.includes(tagId));

      if (onClose && prev && hasChanged) {
        onClose(selectedTags);
        initialValue.current = selectedTags;
      }

      return !prev;
    });
  }

  function handleTagSelect(tagId: number) {
    if (selectedTags.includes(tagId)) {
      setSelectedTags((prev) => prev.filter((id) => id !== tagId));
    } else {
      setSelectedTags((prev) => [...prev, tagId]);
    }
  }

  function handleTagRemove(tagId: number) {
    setSelectedTags((prev) => prev.filter((id) => id !== tagId));
  }

  return (
    <Tags open={isOpen} onOpenChange={handleIsOpenChange}>
      <TagsTrigger disabled={disabled || !canAddMore}>
        {selectedTags.map((tag) => (
          <TagsValue
            key={tag}
            onRemove={() => handleTagRemove(tag)}
            color={availableTags.find((t) => t.id === tag)?.color || "#000000"}
          >
            {availableTags.find((t) => t.id === tag)?.name}
          </TagsValue>
        ))}
      </TagsTrigger>
      <TagsContent>
        <TagsInput placeholder="Search tag..." />
        <TagsList>
          <TagsEmpty />
          <TagsGroup>
            {availableTags.map((tag) => (
              <TagsItem
                key={tag.id}
                value={tag.id.toString()}
                onSelect={() => handleTagSelect(tag.id)}
              >
                <div className="flex items-center gap-2">
                  <span
                    className="size-2 rounded-sm"
                    style={{ backgroundColor: tag.color }}
                  />

                  {tag.name}
                </div>

                {selectedTags.includes(tag.id) && (
                  <CheckIcon size={14} className="text-muted-foreground" />
                )}
              </TagsItem>
            ))}
          </TagsGroup>
        </TagsList>
      </TagsContent>
    </Tags>
  );
}
