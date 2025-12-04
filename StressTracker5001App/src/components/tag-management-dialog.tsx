import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import type { TagCreateDto, TagDto, TagUpdateDto } from "@/dto/tag.dto";
import { TAG_COLORS } from "@/dto/tag.dto";
import { useConfirm } from "@/hooks/use-confirm";
import { useTagCreateMutation } from "@/hooks/use-tag-create-mutation";
import { useTagDeleteMutation } from "@/hooks/use-tag-delete-mutation";
import { useTagUpdateMutation } from "@/hooks/use-tag-update-mutation";
import { TagSchema } from "@/schemas/tag";
import { useForm } from "@tanstack/react-form";
import { Pencil, Tags, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { ColorPickerInput } from "./color-picker-input";
import { TagBadge } from "./tag-badge";
import { Field, FieldError, FieldGroup, FieldLabel } from "./ui/field";

interface TagManagementDialogProps {
  boardId: number;
  tags: TagDto[];
}

export function TagManagementDialog({
  boardId,
  tags,
}: TagManagementDialogProps) {
  const [open, setOpen] = useState(false);
  const [editingTag, setEditingTag] = useState<TagDto | null>(null);
  const form = useForm({
    defaultValues: {
      name: editingTag?.name || "",
      color: editingTag?.color || TAG_COLORS[0],
    },
    validators: {
      onSubmit: TagSchema,
    },
    onSubmit: ({ value }) => {
      if (editingTag) {
        const data: TagUpdateDto = {
          name: value.name.trim(),
          color: value.color,
        };
        updateTagMutation.mutate({ tagId: editingTag.id, ...data });
      } else {
        if (tags.length >= 20) {
          toast.error("Board has reached the maximum limit of 20 tags");
          return;
        }

        const data: TagCreateDto = {
          color: value.color,
          name: value.name.trim(),
          boardId,
        };
        createTagMutation.mutate(data);
      }

      resetForm();
    },
  });

  useEffect(() => {
    if (editingTag) {
      form.reset({
        name: editingTag.name,
        color: editingTag.color,
      });
    }
  }, [editingTag, form]);

  const [ConfirmDialog, confirm] = useConfirm({
    title: "Delete Tag",
    message:
      "Are you sure you want to delete this tag? This will remove it from all cards.",
  });

  const createTagMutation = useTagCreateMutation(boardId);
  const updateTagMutation = useTagUpdateMutation(boardId);
  const deleteTagMutation = useTagDeleteMutation(boardId);

  const resetForm = () => {
    form.reset();
    setEditingTag(null);
  };

  const handleDelete = async (tag: TagDto) => {
    const confirmed = await confirm();

    if (confirmed) {
      deleteTagMutation.mutate(tag);
    }
  };

  return (
    <>
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogTrigger asChild>
          <Button variant="outline" size="sm">
            <Tags className="h-4 w-4 mr-2" />
            Manage Tags
          </Button>
        </DialogTrigger>
        <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Manage Tags</DialogTitle>
            <DialogDescription>
              Create and manage tags for your board. Maximum 20 tags per board.
            </DialogDescription>
          </DialogHeader>

          <form
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();

              form.handleSubmit(e);
            }}
          >
            <FieldGroup>
              <form.Field
                name="name"
                children={(field) => {
                  const isInvalid =
                    field.state.meta.isTouched && !field.state.meta.isValid;
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor={field.name}>Tag Name</FieldLabel>
                      <Input
                        id={field.name}
                        name={field.name}
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        placeholder="Enter tag name"
                        maxLength={50}
                      />
                      {isInvalid && (
                        <FieldError errors={field.state.meta.errors} />
                      )}
                    </Field>
                  );
                }}
              />

              <form.Field
                name="color"
                children={(field) => {
                  const isInvalid =
                    field.state.meta.isTouched && !field.state.meta.isValid;
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor={field.name}>Color</FieldLabel>

                      <div className="space-y-3">
                        <ColorPickerInput
                          defaultValue={TAG_COLORS[0]}
                          value={field.state.value}
                          onChange={field.handleChange}
                        />
                        <div className="space-y-2">
                          <p className="text-xs text-muted-foreground">
                            Preset colors:
                          </p>
                          <div className="grid grid-cols-11 gap-2">
                            {TAG_COLORS.map((color) => (
                              <button
                                key={color}
                                type="button"
                                onClick={() => field.handleChange(color)}
                                style={{ backgroundColor: color }}
                                className={`w-8 h-8 rounded-md transition-all ${
                                  field.state.value === color
                                    ? "ring-2 ring-offset-2 ring-primary scale-110"
                                    : "hover:scale-105"
                                }`}
                                title={color}
                              />
                            ))}
                          </div>
                        </div>
                      </div>

                      {isInvalid && (
                        <FieldError errors={field.state.meta.errors} />
                      )}
                    </Field>
                  );
                }}
              />

              <Field>
                <form.Subscribe
                  selector={(s) => !s.canSubmit && s.isSubmitting}
                >
                  {(disabled) => (
                    <Button type="submit" disabled={disabled}>
                      {editingTag ? "Update Tag" : "Create Tag"}
                    </Button>
                  )}
                </form.Subscribe>
                {editingTag && (
                  <Button type="button" variant="outline" onClick={resetForm}>
                    Cancel
                  </Button>
                )}
              </Field>
            </FieldGroup>
          </form>

          <TagList
            tags={tags}
            setEditingTag={setEditingTag}
            handleDelete={handleDelete}
          />
        </DialogContent>
      </Dialog>
      <ConfirmDialog />
    </>
  );
}

interface TagListProps {
  tags: TagDto[];
  setEditingTag: (tag: TagDto) => void;
  handleDelete: (tag: TagDto) => void;
}

function TagList({ tags, setEditingTag, handleDelete }: TagListProps) {
  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-medium">
          Existing Tags ({tags.length}/20)
        </h4>
      </div>
      {tags.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          No tags yet. Create your first tag above.
        </p>
      ) : (
        <div className="space-y-2">
          {tags.map((tag) => (
            <div
              key={tag.id}
              className="flex items-center justify-between p-2 border rounded-lg"
            >
              <TagBadge tag={tag} />
              <div className="flex gap-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setEditingTag(tag)}
                >
                  <Pencil className="h-4 w-4" />
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleDelete(tag)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
