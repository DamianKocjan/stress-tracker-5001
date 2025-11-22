import {
  BarChart3,
  CheckCircle,
  Columns3,
  LayoutDashboard,
  ListChecks,
  Users,
} from "lucide-react";
import { Card, CardContent } from "./ui/card";

export function FeaturesSection() {
  return (
    <section id="features" className="py-20 sm:py-28 bg-muted/30">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-16">
          <h2 className="text-3xl sm:text-4xl font-bold text-foreground mb-4 text-balance animate-fade-up">
            Everything You Need to Stay Organized
          </h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto text-pretty leading-relaxed animate-fade-up delay-100">
            Powerful features designed to help you manage tasks efficiently and
            reduce overwhelm
          </p>
        </div>

        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8">
          {[
            {
              icon: Columns3,
              title: "Kanban Boards",
              description:
                "Visualize your workflow with customizable columns. Drag and drop tasks between stages effortlessly.",
              delay: "delay-100",
            },
            {
              icon: ListChecks,
              title: "Smart To-Do Lists",
              description:
                "Create detailed task lists with priorities, due dates, and custom labels to stay on track.",
              delay: "delay-200",
            },
            {
              icon: BarChart3,
              title: "Progress Tracking",
              description:
                "Monitor completion rates and productivity trends with visual charts and insights.",
              delay: "delay-300",
            },
            {
              icon: Users,
              title: "Team Collaboration",
              description:
                "Assign tasks, share boards, and collaborate seamlessly with your team members.",
              delay: "delay-400",
            },
            {
              icon: CheckCircle,
              title: "Stress Reduction",
              description:
                "Clear visualization reduces anxiety by showing exactly what needs attention and what's complete.",
              delay: "delay-500",
            },
            {
              icon: LayoutDashboard,
              title: "Multiple Views",
              description:
                "Switch between Kanban, list, and calendar views to match your workflow preferences.",
              delay: "delay-600",
            },
          ].map((feature, index) => (
            <Card
              key={index}
              className={`border-border hover:border-foreground/20 transition-all duration-300 hover:shadow-lg hover:-translate-y-1 animate-scale-in ${feature.delay}`}
            >
              <CardContent className="pt-6">
                <div className="h-12 w-12 rounded-lg bg-foreground/5 flex items-center justify-center mb-4 group-hover:bg-foreground/10 transition-colors">
                  <feature.icon className="h-6 w-6 text-foreground" />
                </div>
                <h3 className="text-xl font-semibold text-foreground mb-2">
                  {feature.title}
                </h3>
                <p className="text-muted-foreground leading-relaxed">
                  {feature.description}
                </p>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </section>
  );
}
